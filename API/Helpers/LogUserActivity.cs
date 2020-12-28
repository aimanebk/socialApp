using System;
using System.Threading.Tasks;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContex = await next();

            if(!resultContex.HttpContext.User.Identity.IsAuthenticated) return;

            var userId = resultContex.HttpContext.User.GetUserId();

            var repo = resultContex.HttpContext.RequestServices.GetService<IUserRepository>();
            var user = await repo.GetUserByIdAsync(userId);
            user.LasActive = DateTime.Now;

            await repo.SaveAllAsync();
        }
    }
}