using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TechBuddyAPI.Data;
using TechBuddyAPI.Models;

namespace TechBuddyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TechBuddyContext _context;
        private readonly IConfiguration _config;
        public AuthController(TechBuddyContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private string HashPassword(string password)
        {
            SHA256 hash = SHA256.Create();
            var passwordBytes = Encoding.Default.GetBytes(password);
            var hashedPassword = hash.ComputeHash(passwordBytes);
            return Convert.ToHexString(hashedPassword);
        }

        private Users AuthenticateUser(Users user)
        {
            Users _user = null;

            var userMatch = _context.Users.Any(u => u.Username == user.Username);

            if(userMatch)
            {
                var userLogin = _context.Users.FirstOrDefault(u => u.Username == user.Username);
                if (HashPassword($"{user.Password}{userLogin.Salt}") == userLogin.Password)
                {
                    _user = new Users
                    {
                        Username = user.Username
                    };
                }
            }

            return _user;
        }

        private string GenerateToken(Users user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
            };


            var token = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Audience"], claims: claims, expires: DateTime.Now.AddMinutes(10), signingCredentials: credentials); 
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(Users user)
        {
            IActionResult response = Unauthorized();
            var _user = AuthenticateUser(user);
            if(_user != null)
            {
                var token = GenerateToken(user);
                response = Ok(new { token = token });
            }
            return response;
        }

 
        [HttpPost("Register")]

        public async Task<IActionResult> Register([FromBody] Users user)
        {
            try
            {

                var salt = DateTime.Now.ToString();
                var hashedPW = HashPassword($"{user.Password}{salt}");

                _context.Users.Add(new Users() { Username = user.Username, Password = hashedPW, Salt = salt });
                await _context.SaveChangesAsync();
            }

            catch (Exception e)
            {
                    string message = ("En bruger er allerede oprettet med brugernavnet " + user.Username);
                    return Conflict(message);   
            }
            return Ok(user);
        }


        [Authorize]
        [HttpGet("GetUser")]

        public ActionResult<string> GetUser()
        {
            try
            {
                return Ok(new { authenticated = true, user = User?.Identity.Name });
            }
            catch (Exception e)
            {
                return Conflict(e.Message);
            }
        }


    }
}
