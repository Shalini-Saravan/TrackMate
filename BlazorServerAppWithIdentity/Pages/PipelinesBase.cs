using BlazorServerAppWithIdentity.Models;
using BlazorServerAppWithIdentity.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Security.Claims;

namespace BlazorServerAppWithIdentity.Pages
{
    public class PipelinesBase : ComponentBase
    {
        [Inject]
        public AzureService AzureService { get; set; }
        [Inject]
        public IGlobalStateService GlobalStateService { get; set; }
        [Inject]
        public IHttpContextAccessor HttpContextAccessor { get; set; }
        [Inject]
        public UserService? UserService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public IEnumerable<Pipeline> PipelinesList { get; set; } = null;

        public string? LinkHolder { get; set; }
        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            if (GlobalStateService.PAT == null && (HttpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated ?? false))
            {
                string id = HttpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
                ApplicationUser appuser = UserService.GetUserById(id);
                if (appuser != null && appuser.PAT == null)
                {
                    NavigationManager.NavigateTo("/pat");
                }
                else if (GlobalStateService != null)
                {
                    GlobalStateService.PAT = appuser.PAT;
                }
            }
            if (PipelinesList == null)
            {
                PipelinesList = (await AzureService.GetPipelines()).ToList();
            }

        }


    }
}
