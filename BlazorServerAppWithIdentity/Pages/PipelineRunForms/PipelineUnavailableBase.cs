using Microsoft.AspNetCore.Components;

namespace BlazorServerAppWithIdentity.Pages.PipelineRunForms
{
    public class PipelineUnavailableBase : ComponentBase
    {
        [Parameter]
        public string PipeLineId { get; set; }
        [Parameter]
        public string PipeLineName { get; set; }

        public PipelineUnavailableBase()
        {
            PipeLineId = Guid.NewGuid().ToString();
            PipeLineName = Guid.NewGuid().ToString();
        }
    }
}
