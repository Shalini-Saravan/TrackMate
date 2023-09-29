using Blazored.LocalStorage;
using BlazorServerAppWithIdentity.Models;
using BlazorServerAppWithIdentity.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.JSInterop;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BlazorServerAppWithIdentity.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {

        private readonly AccountService AccountService;
        private readonly UserService UserService;
        public LoginModel(AccountService AccountService, UserService UserService)
        {
            this.AccountService = AccountService;
            this.UserService = UserService;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public string ReturnUrl { get; set; }

        public Boolean isSubmitting { get; set; } = false;

        public void OnGet()
        {
            ReturnUrl = Url.Content("~/");
            ViewData["Message"] = "";
        }
        public IActionResult OnPost()
        {
            if (isSubmitting) return Page();
            isSubmitting = true;

            ReturnUrl = Url.Content("~/token?token=");
            try
            {
                if (ModelState.IsValid)
                {

                    var result = AccountService.Login(Input.UserName, Input.Password);
                    if (result != null)
                    {
                        return LocalRedirect(ReturnUrl+result);
                    }
                }
                ViewData["Message"] = "Login Failed: Invalid Email or Password";
                ModelState.AddModelError(nameof(Input.UserName), "Login Failed: Invalid Email or Password");

                return Page();
            }
            finally { isSubmitting = false; }
        }
       
        public class InputModel
        {
            [Required]
            public string UserName { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

    }
}
