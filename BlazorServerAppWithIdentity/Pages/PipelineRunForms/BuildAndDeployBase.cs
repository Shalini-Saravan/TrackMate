using BlazorServerAppWithIdentity.Models;
using BlazorServerAppWithIdentity.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace BlazorServerAppWithIdentity.Pages.PipelineRunForms
{
    public class BuildAndDeployBase : ComponentBase
    {
        [Inject]
        public AzureService? AzureService { get; set; }

        [Parameter]
        public string? PipeLineId { get; set; }
        [Parameter]
        public string? PipeLineName { get; set; }

        public string TSBuildAgentDeploymentConfigFileName { get; set; } = "TestStand-BuildAgent-Production";
        public string TSBuildAgentName { get; set; } = "TS-BA-IAC-PD-5";

        public Boolean isSubmitting { get; set; } = false;
        public String notification { get; set; } = "";
        public string? runsLink { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            runsLink = "/pipeline/" + PipeLineId + "/" + PipeLineName + "/runs";

        }

        public void RunPipeline()
        {
            if (isSubmitting) return;
            isSubmitting = true;
            try
            {
                string jsonString = "{\"definition\":{\"id\":" + PipeLineId + "}, \"templateParameters\": {\"TSBuildAgentDeploymentConfigFileName\": \"" + TSBuildAgentDeploymentConfigFileName + "\", \"TSBuildAgentName\": \"" + TSBuildAgentName + " \"}}";

                var result = AzureService?.RunPipeline(jsonString, PipeLineId ?? "");
                if (result == null || !result.IsSuccessStatusCode)
                {
                    notification = "Error: Unable to run pipeline!";
                }
                else
                {
                    notification = "Pipeline Run Success!";
                }
            }
            finally { isSubmitting = false; }
        }
        protected void clearNotification()
        {
            this.notification = string.Empty;
        }
    }
}
