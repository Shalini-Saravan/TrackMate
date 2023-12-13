using TrackMate.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.JSInterop;

namespace TrackMate.Pages
{
    public class TokenBase : ComponentBase
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public AzureService AzureService { get; set; }
        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            string token = null;
            StringValues codeParam;
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("token", out codeParam))
            {
                token = Convert.ToString(codeParam);
            }
            await SetApiToken(token);
            AzureService.GetToken();

        }
        public async Task SetApiToken(string token)
        {
            try
            {
                //await LocalStorage.SetItemAsStringAsync("TokenValue", token);
                await JSRuntime.InvokeVoidAsync("localStorage.setItem", "TokenValue", token);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception on Token setItem" + ex.Message);
            }
        }
    }
}
