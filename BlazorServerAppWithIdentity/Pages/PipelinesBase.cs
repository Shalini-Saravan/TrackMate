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
            if (PipelinesList == null)
            {
                PipelinesList = (await AzureService.GetPipelines()).ToList();
            }

        }


    }
}
