using Microsoft.AspNetCore.Components;
using TrackMate.Models;
using TrackMate.Services;

namespace TrackMate.Pages
{
    public class RunsBase : ComponentBase
    {
        [Parameter]
        public string PipeLineId { get; set; }
        [Parameter]
        public string PipeLineName { get; set; }
        [Inject]
        public AzureService AzureService { get; set; }

        public List<Run>? runsList { get; set; }

        protected override async Task OnInitializedAsync()
        {
            base.OnInitializedAsync();
            runsList = await AzureService.GetRunsWithPipelineId(PipeLineId);
        }


    }
}
