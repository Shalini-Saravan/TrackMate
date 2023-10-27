using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrackMate.Models;
using TrackMate.Services;

namespace TrackMate.Pages
{
    public class IndexBase : ComponentBase
    {

        [Inject]
        public MachineService MachineService { get; set; }

        [Inject]
        public UserService UserService { get; set; }

        [Inject]
        public MachineUsageService MachineUsageService { get; set; }

        [Inject]
        AuthenticationStateProvider auth { get; set; }

        [Inject]
        NavigationManager NavigationManager { get; set; }
        [Inject]
        public IHttpContextAccessor HttpContextAccessor { get; set; }
        public int UserCount { get; set; }
        public int MachineCount { get; set; }
        public int InUseCount { get; set; }
        public int AvailableCount { get; set; }
        public string Message { get; set; } = string.Empty;

        public Machine machine { get; set; }


        public IEnumerable<MachineUsage> MachineUsageLogs { get; set; } = new List<MachineUsage>();

        protected override async Task OnInitializedAsync()
        {
            base.OnInitializedAsync();

            try
            {
                UserCount = UserService.GetUsersCount();
                MachineCount = MachineService.GetMachineCount();
                AvailableCount = MachineService.GetAvailableMachineCount();
                InUseCount = MachineCount - AvailableCount;

                var authstate = await auth.GetAuthenticationStateAsync();
                var user = authstate.User;

                if (user.Identity.IsAuthenticated)
                {
                    MachineUsageLogs = MachineUsageService.GetMachineUsageByUserName(user?.Identity?.Name ?? "");
                }

            }
            catch (Exception)
            {
                UserCount = 0;
                MachineCount = 0;
                AvailableCount = 0;
                InUseCount = MachineCount - AvailableCount;
            }

        }

        public void GetMachine(string id)
        {
            machine = MachineService.GetMachineById(id);
        }

    }
}
