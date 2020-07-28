using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiAuth.Models;

namespace WebApiAuth.Services
{
    public interface IUserService
    {
        Task<WebApiAuthUser> GetUserAsync(Guid userId);

        Task<WebApiAuthUser> GetUserAsync(string userId);

        Task<WebApiAuthUser> GetUserByUserName(string userName);

        IEnumerable<WebApiAuthUser> GetAll(string userNameFilter);
    }
}
