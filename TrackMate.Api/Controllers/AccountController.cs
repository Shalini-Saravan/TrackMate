using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TrackMate.Api.Services;

namespace TrackMate.Api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AccountController : ControllerBase
    {
        private readonly AccountService AccountService;

        public AccountController(AccountService accountService)
        {
            AccountService = accountService;
        }

        [HttpGet("login/{userName}/{password}")]
        public JsonResult Login(string userName, string password)
        {
            string result = AccountService.Login(userName, password).Result;
            return new JsonResult(result);
        }

        [HttpPost("changepassword")]
        public string ChangePassword([FromBody] string strdata)
        {
            JObject data = JObject.Parse(strdata);
            string newPassword = data["newPassword"].ToString();
            string oldPassword = data["oldPassword"].ToString();
            string userName = data["userName"].ToString();

            var result = AccountService.ChangePassword(newPassword, oldPassword, userName).Result;
            if (result != null && result.Succeeded)
            {
                return "true";
            }
            return "false";
        }

    }

}
