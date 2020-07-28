using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiAuth.Models;

namespace WebApiAuth.Services
{
    public class UserService : IUserService
    {
        private readonly WebApiAuthUserContext dataContext;

        public UserService(WebApiAuthUserContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public  IEnumerable<WebApiAuthUser> GetAll(string userNameFilter)
        {
            var users = this.dataContext.Users.AsQueryable();
            if(!string.IsNullOrEmpty(userNameFilter))
            {
                users = users.Where(u => u.UserName.Contains(userNameFilter));
            }

            return users;
        }

        public async Task<WebApiAuthUser> GetUserAsync(string userId)
        {
            var id = Guid.Parse(userId);
            return await this.dataContext.Users.FindAsync(id);
        }

        public async Task<WebApiAuthUser> GetUserAsync(Guid userId)
        {
            return await this.dataContext.Users.FindAsync(userId);
        }

        public async Task<WebApiAuthUser> GetUserByUserName(string userName)
        {
            return await this.dataContext.Users.SingleOrDefaultAsync(u => u.UserName == userName);
        }
    }
}
