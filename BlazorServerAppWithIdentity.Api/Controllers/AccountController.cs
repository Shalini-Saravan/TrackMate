using BlazorServerAppWithIdentity.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorServerAppWithIdentity.Api.Controllers
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

    }

}
