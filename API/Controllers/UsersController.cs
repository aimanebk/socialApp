using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            this.mapper = mapper;
            this.userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> getUsers()
        {
            var users = await userRepository.GetMembersAsync();

            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> getUser(string username)
        {
            return await userRepository.GetMemberByUsernameAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> updateUser(MemberUpdateDto memberUpateDto){
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await userRepository.GetUserByUsernameAsync(username);

            mapper.Map(memberUpateDto, user);

            userRepository.Update(user);

            if(await userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user");

        }
    }
}