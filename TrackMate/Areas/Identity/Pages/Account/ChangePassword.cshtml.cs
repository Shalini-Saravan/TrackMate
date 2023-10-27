using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TrackMate.Services;

namespace TrackMate.Areas.Identity.Pages.Account
{
    public class ChangePasswordModel : PageModel
    {
        private readonly AccountService AccountService;
        private readonly string UserName;

        public ChangePasswordModel(AccountService AccountService, IHttpContextAccessor HttpContextAccessor)
        {
            this.AccountService = AccountService;
            UserName = HttpContextAccessor.HttpContext?.User?.Identity?.Name ?? "";
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public string ReturnUrl { get; set; }
        public bool isSubmitting { get; set; } = false;

        public void OnGet()
        {
            ReturnUrl = Url.Content("~/");
            ViewData["Message"] = "";
        }
        public IActionResult OnPost()
        {
            if (isSubmitting) return Page();
            isSubmitting = true;
            ReturnUrl = Url.Content("~/Identity/Account/Logout");
            try
            {
                if (ModelState.IsValid)
                {

                    var result = AccountService.ChangePassword(Input.NewPassword, Input.OldPassword, UserName);
                    if (result == "true")
                    {
                        return LocalRedirect(ReturnUrl);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("failed --");
                    }
                }
                ViewData["Message"] = "Failed: Enter Valid Password";

                return Page();
            }
            finally { isSubmitting = false; }
        }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string OldPassword { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; }
        }
    }
}
