using BlazorServerAppWithIdentity.Api.Data;
using BlazorServerAppWithIdentity.Api.Hubs;
using BlazorServerAppWithIdentity.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using System.Security.Claims;
using System.Security.Policy;

namespace BlazorServerAppWithIdentity.Services
{
    public class UserService
    {
        private readonly BlazorServerAppWithIdentityContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IMongoClient client)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            db = new BlazorServerAppWithIdentityContext(client);
        }

        public List<ApplicationRole> GetRoles()
        {
            return _roleManager.Roles.ToList();
        }
        public List<ApplicationUser> GetUsers()
        {
            return _userManager.Users.ToList();
        }
        public int GetUsersCount()
        {
            return _userManager.Users.Count();
        }
        public async Task<ApplicationUser> GetUserById(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }
        
        public async Task<string> AddUser(User user)
        {
            ApplicationUser appUser = new ApplicationUser
            {
                Name = user.Name,
                UserName = user.Email.Substring(0, user.Email.IndexOf('@')),
                Email = user.Email,
                Department = user.Department,
                Team = user.Team
            };

            IdentityResult result = await _userManager.CreateAsync(appUser, user.Password);

            if (result.Succeeded)
            {   
                await _userManager.AddToRoleAsync(appUser, user.Roles);
                return "User added Successfully!";
            }

            return "Failed to add new user!";

        }
        
        public async Task<string> AddRole(ApplicationRole role)
        {
            IdentityResult result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                return "Role added Successfully!";
            }
            else
            {
                return "Failed to add new Role!";
            }
        }
        public async Task<string> EditUser(User user, string uid)
        {
            ApplicationUser userEdit = GetUserById(uid).Result;
            userEdit.Name = user.Name;
            userEdit.UserName = user.Email.Substring(0, user.Email.IndexOf('@'));
            userEdit.Email = user.Email;
            userEdit.Department = user.Department;
            userEdit.Team = user.Team;

            IdentityResult result = await _userManager.UpdateAsync(userEdit);
            if (result.Succeeded)
            {
                return "User updated Successfully!";
            }
            return "Failed to update user!";

        }
        public async Task<string> DeleteUser(ApplicationUser user)
        {
            IdentityResult result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return "User deleted Successfully!";
            }
            return "Failed to delete user!";
        }

    }
}
