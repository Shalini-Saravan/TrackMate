using TrackMate.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Http;

namespace TrackMate.Pages
{
    public class TokenBase : ComponentBase
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public ILocalStorageService LocalStorage { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            string token = null;
            StringValues codeParam;
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("token", out codeParam))
            {
                token = Convert.ToString(codeParam);
            }
            SetApiToken(token);
            NavigationManager.NavigateTo("/");

        }
        public async void SetApiToken(string token)
        {
            await LocalStorage.SetItemAsStringAsync("TokenValue", token);
        }
    }
}
