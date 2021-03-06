using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikeRepository : ILikesRepository
    {
        private readonly DataContext context;
        public LikeRepository(DataContext context)
        {
            this.context = context;
        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
        {
            return await context.Likes.FindAsync(sourceUserId, likedUserId);
        }

        public async Task<PageList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            var users = context.Users.OrderBy(x => x.UserName).AsQueryable();
            var likes = context.Likes.AsQueryable();

            if(likesParams.Predicate == "liked"){
                likes = likes.Where(l => l.SourceUserId == likesParams.UserId);
                users = likes.Select(l => l.LikedUser);
            }
            else if (likesParams.Predicate == "likedBy"){
                likes = likes.Where(l => l.LikedUserId == likesParams.UserId);
                users = likes.Select(l => l.SourceUser);
            }

            var likedUsers = users.Select(user => new LikeDto{
                Id = user.Id,
                Username = user.UserName,
                Age = user.DateOfBirth.CalculateAge(),
                City = user.City,
                KnownAs = user.KnownAs,
                PhotoUrl = user.Photos.SingleOrDefault(p => p.IsMain).Url
            });

            return await PageList<LikeDto>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await context.Users
                        .Include(x => x.LikedUsers)
                        .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}