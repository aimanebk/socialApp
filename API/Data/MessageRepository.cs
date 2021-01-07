using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessagesRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            this.mapper = mapper;
            this.context = context;
        }

        public void AddGroup(Group group)
        {
            context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await context.Connections. FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await context.Groups
                        .Include(c => c.Connections)
                        .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
                        .FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int id)
        {
            return await context.Messages.FindAsync(id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await context.Groups
                    .Include(x => x.Connections)
                    .FirstOrDefaultAsync(x => x.Name == groupName);         
        }

        public async Task<PageList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = context.Messages
                        .OrderByDescending(m => m.MessageSent)
                        .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(m => m.RecipientUsername == messageParams.Username && m.RecipientDeleted == false),
                "Outbox" => query.Where(m => m.SenderUsername == messageParams.Username && m.SenderDeleted == false),
                _ => query.Where(m => m.RecipientUsername == messageParams.Username && m.RecipientDeleted == false && m.DateRead == null)
            };

            var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);

            return await PageList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string senderUsername, string recipientUsername)
        {
            var messages = await context.Messages
                                .Include(m => m.Sender).ThenInclude(s => s.Photos)
                                .Include(m => m.Recipient).ThenInclude(r => r.Photos)
                                .Where(m => m.Recipient.UserName == senderUsername && m.RecipientDeleted == false &&
                                            m.Sender.UserName == recipientUsername ||
                                            m.Recipient.UserName == recipientUsername &&
                                            m.Sender.UserName == senderUsername && m.SenderDeleted == false)
                                            .OrderBy(m => m.MessageSent)
                                            .ToListAsync();

            var unReadMessages = messages.Where(m => m.DateRead == null &&
                             m.Recipient.UserName == senderUsername).ToList();

            if (unReadMessages.Any())
            {
                foreach (var unReadMessage in unReadMessages)
                {
                    unReadMessage.DateRead = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();

            }

            return mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public void RemoveConnection(Connection connection)
        {
            context.Connections.Remove(connection);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}