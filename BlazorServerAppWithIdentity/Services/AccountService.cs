using BlazorServerAppWithIdentity.Models;
using BlazorServerAppWithIdentity.Pages;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace BlazorServerAppWithIdentity.Services
{
    public class AccountService
    {
        private readonly HttpClient httpClient;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IGlobalStateService GlobalState;
        private readonly IHttpContextAccessor HttpContextAccessor;
        private readonly UserService UserService;
        public AccountService(UserService UserService, IHttpContextAccessor HttpContextAccessor, HttpClient httpClient, SignInManager<ApplicationUser> signInManager, IGlobalStateService globalState)
        {
            this.httpClient = httpClient;
            _signInManager = signInManager;
            GlobalState = globalState;
            this.HttpContextAccessor = HttpContextAccessor;
            this.UserService = UserService;
        }

        public SignInResult Login(string userName, string password)
        {
            string response = httpClient.GetFromJsonAsync<string>("api/account/login/" + userName + "/" + password).Result;
            var result = _signInManager.PasswordSignInAsync(userName, password, false, false);
            GlobalState.TokenValue = response ?? "";

            return result.Result;
        }
       
    }
}
