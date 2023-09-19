using BlazorServerAppWithIdentity.Services;
using Microsoft.AspNetCore.Components;

namespace BlazorServerAppWithIdentity.Pages
{

    public class PipelineViewBase : ComponentBase
    {

        [Parameter]
        public string? PipeLineId { get; set; }
        [Parameter]
        public string? PipeLineName { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }
    }
}
