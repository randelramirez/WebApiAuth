using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebApiAuth.Models;

namespace WebApiAuth.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly Microsoft.AspNetCore.Identity.UserManager<WebApiAuthUser> userManager;
        private readonly IConfiguration configuration;

        public UsersController(UserManager<WebApiAuthUser> userManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("authenticate1")]
        public async Task<IActionResult> Authenticate1(AuthenticateModel model)
        {

            var user = await userManager.FindByNameAsync(model.Username);

            if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
            {
                // Flow from Pluralsight: Building a Web App with ASP.NET Core, MVC, Entity Framework Core, Bootstrap, and Angular
                var claims = new Claim[]
                {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
                };

                //var tokenKeyConfig = Encoding.UTF8.GetBytes(configuration.GetSection("Token:Key").Value);
                var tokenKeyConfig = configuration["Tokens:Key"];
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKeyConfig));
                var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(issuer: configuration["Tokens:Issuer"],
                    audience: configuration["Tokens:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(30),
                    signingCredentials: signingCredentials);

                var result = new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                };

                return Ok(result);
            }
            else
            {
                return BadRequest();
            }
        }

        [AllowAnonymous]
        [HttpPost("authenticate2")]
        public async Task<IActionResult> Authenticate2([FromBody] AuthenticateModel model)
        {
            //var user = userManager.Authenticate(model.Username, model.Password);
            var user = await userManager.FindByNameAsync(model.Username);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            if(await userManager.CheckPasswordAsync(user, model.Password))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(configuration["Tokens:Key"]);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        // We use the userId as the name
                        new Claim(ClaimTypes.Name, user.Id.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                // return basic user info and authentication token
                return Ok(new
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Token = tokenString
                });
            }

            return BadRequest();
           
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var result = await userManager.CreateAsync(new WebApiAuthUser() { Email = model.Email, UserName = model.UserName }, model.Password);
            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        private async Task<WebApiAuthUser> Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            //var user = _context.Users.SingleOrDefault(x => x.Username == username);
            var user = await userManager.FindByNameAsync(username);

            // check if username exists
            if (user == null)
                return null;

            // check if password is correct
            //if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            //    return null;
            if (!await userManager.CheckPasswordAsync(user, password))
                return null;

            // authentication successful
            return user;
        }
    }
}
