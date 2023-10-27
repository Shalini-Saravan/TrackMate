using Microsoft.AspNetCore.Components;
using System;
using System.Security.Claims;
using TrackMate.Models;
using TrackMate.Services;

namespace TrackMate.Pages
{
    public class PipelinesBase : ComponentBase
    {
        [Inject]
        public AzureService AzureService { get; set; }

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
