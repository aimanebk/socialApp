using System.Collections.Generic;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IMapper mapper;
        private readonly IUserRepository userRepository;
        private readonly IMessagesRepository messageRepository;
        public MessagesController(IUserRepository userRepository, IMessagesRepository messageRepository, IMapper mapper)
        {
            this.messageRepository = messageRepository;
            this.userRepository = userRepository;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> AddMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();

            if (username == createMessageDto.RecipientUsername)
                BadRequest("You cannot send message to yourself");

            var sender = await userRepository.GetUserByUsernameAsync(username);
            var recipient = await userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) return NotFound();

            var message = new Message
            {
                Content = createMessageDto.Content,
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName
            };

            messageRepository.AddMessage(message);

            if (await messageRepository.SaveAllAsync()) return Ok(mapper.Map<MessageDto>(message));

            return BadRequest("Failed to add message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams){
            messageParams.Username = User.GetUsername();

            var messages = await messageRepository.GetMessagesForUser(messageParams);

            Response.AddpaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesThread(string username){
          var currentUsername = User.GetUsername();

          return Ok(await messageRepository.GetMessageThread(currentUsername, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id){
            var username = User.GetUsername();

            var message = await messageRepository.GetMessage(id);

            if(message.SenderUsername != username && message.RecipientUsername != username)
                return Unauthorized();

            if(message.SenderUsername == username) message.SenderDeleted = true;

            if(message.RecipientUsername == username) message.RecipientDeleted = true;

            if(message.SenderDeleted && message.RecipientDeleted) messageRepository.DeleteMessage(message);

            if(await messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to delete message");
        }
    }
}