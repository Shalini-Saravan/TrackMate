using Azure;
using Blazored.LocalStorage;
using TrackMate.Pages;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using TrackMate.Models;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace TrackMate.Services
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
        public string ChangePassword(string newPassword, string oldPassword, string userName)
        {
            var jsonString = "{'newPassword' :'" + newPassword + "', 'oldPassword' : '" + oldPassword + "', 'userName' : '" + userName + "'}";
            var response = httpClient.PostAsJsonAsync("api/account/changepassword", jsonString).Result;
            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                return responseContent.ToString();
            }
            else
            {
                return "false";
            }

        }


    }
}
