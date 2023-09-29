using Azure;
using Blazored.LocalStorage;
using BlazorServerAppWithIdentity.Models;
using BlazorServerAppWithIdentity.Pages;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using System.Security.Claims;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace BlazorServerAppWithIdentity.Services
{
    public class AccountService
    {
        private readonly HttpClient httpClient;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILocalStorageService _localStorage;
        protected IJSRuntime JSRuntime;
        public AccountService(IJSRuntime JSRuntime, ILocalStorageService localStorage, HttpClient httpClient, SignInManager<ApplicationUser> signInManager)
        {
            this.httpClient = httpClient;
            _signInManager = signInManager;
            _localStorage = localStorage;
            this.JSRuntime = JSRuntime;
        }

        public string Login(string userName, string password)
        {
            string response = httpClient.GetFromJsonAsync<string>("api/account/login/" + userName + "/" + password).Result;
            var result = _signInManager.PasswordSignInAsync(userName, password, false, false);
            if (result.Result.Succeeded)
            {
                return response;
            }
            return null;
        }
        

    }
}
