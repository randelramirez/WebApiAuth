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
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApiAuth.Models;
using WebApiAuth.Services;

namespace WebApiAuth.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly Microsoft.AspNetCore.Identity.UserManager<WebApiAuthUser> userManager;
        private readonly IUserService userService;
        //private readonly IOptions<JwtSettings> jwtConfig;
        private readonly JwtOptions jwtConfig;

        public UsersController(UserManager<WebApiAuthUser> userManager, IUserService userService, IOptions<JwtOptions> jwtConfig)
        {
            this.userManager = userManager;
            this.userService = userService;
            this.jwtConfig = jwtConfig.Value;
        }

        // We do not need the FromQuery attribute since this is an API controller, 
        // and there is no route template and the parameter is not a complex type so default binding source with from the query string
        [AllowAnonymous]
        [HttpGet]
        public ActionResult<IEnumerable<WebApiAuthUser>> Get([FromQuery(Name = "q")] string contains)
        {
            // move the userContext to its own service
            var users = this.userService.GetAll(contains);

            if (!string.IsNullOrEmpty(contains))
            {
                users.Where(u => u.UserName.Contains(contains));
            }

            return Ok(users.ToList());
        }

        [AllowAnonymous]
        [HttpGet("{userId}")]
        public async Task<ActionResult<WebApiAuthUser>> Get(Guid userId)
        {
            // if userId is not a Guid, the action will return 400 BadRequest
            var user = await userManager.FindByIdAsync(userId.ToString());
            if(user != null)
            {
                return Ok(user);
            }

            return NotFound();
        }

        [AllowAnonymous]
        [Route(nameof(UsersController.Authenticate1))]
        [HttpPost]
        public async Task<IActionResult> Authenticate1(AuthenticateModel model)
        {

            var user = await userManager.FindByNameAsync(model.Username);

            if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
            {
                var claims = new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
                };

                // claims from [AspNetUserClaims] provided by ASP.NET Core Identity
                var userClaims = await userManager.GetClaimsAsync(user);

                var allClaims = new List<Claim>();
                allClaims.AddRange(claims);
                allClaims.AddRange(userClaims);
               
                //var tokenKeyConfig = Encoding.UTF8.GetBytes(configuration.GetSection("Token:Key").Value);
                var tokenKeyConfig = this.jwtConfig.Key;
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKeyConfig));
                var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(issuer: this.jwtConfig.Issuer,
                    audience: this.jwtConfig.Audience,
                    claims: allClaims,
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
        [Route(nameof(UsersController.Authenticate2))]
        [HttpPost]
        public async Task<IActionResult> Authenticate2([FromBody] AuthenticateModel model)
        {
            //var user = userManager.Authenticate(model.Username, model.Password);
            var user = await userManager.FindByNameAsync(model.Username);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            if (await userManager.CheckPasswordAsync(user, model.Password))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(this.jwtConfig.Key);

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
        [Route(nameof(UsersController.Register))]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var user = new WebApiAuthUser() { Email = model.Email, UserName = model.UserName };
            var result = await userManager.CreateAsync(user, model.Password);
            
            if (result.Succeeded)
            {
                
                await userManager.AddClaimAsync(user, new Claim("CustomClaim", $"{user.NormalizedUserName}/{user.Email}"));
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
