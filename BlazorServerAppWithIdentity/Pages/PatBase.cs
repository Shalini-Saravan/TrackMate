
using BlazorServerAppWithIdentity.Models;
using BlazorServerAppWithIdentity.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Claims;

namespace BlazorServerAppWithIdentity.Pages
{
    public class PatBase : ComponentBase
    {

        [Inject]
        public UserService? UserService { get; set; }
        [Inject]
        public IHttpContextAccessor? HttpContextAccessor { get; set; }

        public string PAT { get; set; }
        public DateTime ValidUpto { get; set; } = DateTime.UtcNow;
        public Boolean isSubmitting { get; set; } = false;
        public String notification { get; set; } = "";

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        public void AddPat()
        {
            if (isSubmitting) return;
            isSubmitting = true;
            try
            {
                ApplicationUser user = UserService.GetUserById(HttpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "");

                user.PAT = PAT;
                user.ValidUpto = ValidUpto;
                var result = UserService.AddPat(user);
                notification = result.ToString();

            }
            catch (Exception)
            {
                notification = "Failed to update PAT!";
            }
            finally { isSubmitting = false; }
        }
        protected void clearNotification()
        {
            this.notification = string.Empty;
        }
    }
}
