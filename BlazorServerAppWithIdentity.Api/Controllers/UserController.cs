using BlazorServerAppWithIdentity.Models;
using BlazorServerAppWithIdentity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace BlazorServerAppWithIdentity.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(AuthenticationSchemes = "Bearer")]
    public class UserController : ControllerBase
    {
        private readonly UserService UserService;
        public UserController(UserService userService)
        {
            UserService = userService;
        }

        [HttpGet]
        public List<ApplicationUser> GetUsers()
        {
            return UserService.GetUsers();
        }

        [HttpGet("Roles")]
        public List<ApplicationRole> GetRoles()
        {
            return UserService.GetRoles();
        }

        [HttpGet("count")]
        public int Count()
        {
            return UserService.GetUsersCount();
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public ApplicationUser GetUserById(string id)
        {
            return UserService.GetUserById(id).Result;
        }


        [HttpPost]
        public JsonResult Add([FromBody] string strData)
        {
            JObject data = JObject.Parse(strData);
            User user = data["user"].ToObject<User>();
            if (user == null)
            {
                return new JsonResult("Failed Operation!");
            }
            else
            {
                return new JsonResult(UserService.AddUser(user));
            }
        }

        [HttpPost("pat")]
        public JsonResult AddPat([FromBody] string strData)
        {
            JObject data = JObject.Parse(strData);
            ApplicationUser user = data["user"].ToObject<ApplicationUser>();
            if (user == null)
            {
                return new JsonResult("Failed Operation!");
            }
            else
            {
                return new JsonResult(UserService.AddPat(user));
            }
        }

        [HttpPost("role")]
        public JsonResult AddRole([FromBody] string strData)
        {
            JObject data = JObject.Parse(strData);
            ApplicationRole role = data["role"].ToObject<ApplicationRole>();
            if (role == null)
            {
                return new JsonResult("Failed Operation!");
            }
            else
            {
                return new JsonResult(UserService.AddRole(role));
            }
        }

        [HttpPut]
        public JsonResult Edit([FromBody] string strData)
        {
            JObject data = JObject.Parse(strData);
            User user = data["user"].ToObject<User>();
            string uid = data["uid"].ToString();
            if (user == null || uid == null)
            {
                return new JsonResult("Failed Operation!");
            }
            else
            {
                return new JsonResult(UserService.EditUser(user, uid));
            }
        }
        [HttpPost("delete")]
        public JsonResult DeleteUser([FromBody] string strData)
        {
            JObject data = JObject.Parse(strData);
            ApplicationUser user = data["user"].ToObject<ApplicationUser>();
            if (user == null)
            {
                return new JsonResult("Failed Operation!");
            }
            else
            {
                return new JsonResult(UserService.DeleteUser(user));
            }
        }

    }

}
