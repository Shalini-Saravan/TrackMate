using BlazorServerAppWithIdentity.Models;
using BlazorServerAppWithIdentity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlazorServerAppWithIdentity.Api.Services
{

    public class AccountService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly UserService UserService;

        public AccountService(UserService UserService, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
            this.UserService = UserService;
        }

        public async Task<string> Login(string userName, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(userName, password, false, false);
            if (result.Succeeded)
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSecurityKey"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userName.ToLower())
                };

                var token = new JwtSecurityToken(_configuration["JwtIssuer"],
                    _configuration["JwtAudience"],
                    claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials);

                var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
                return tokenValue;
            }
            return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken()).ToString();
        }
        public async Task<IdentityResult> ChangePassword(string newPassword, string oldPassword, string userName)
        {
            ApplicationUser user = UserService.GetUserByUserName(userName).Result;
            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            return result;
                
        }

    }
}
