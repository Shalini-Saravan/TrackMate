using BlazorServerAppWithIdentity.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Http;

namespace BlazorServerAppWithIdentity.Pages
{
    public class AuthorizeBase : ComponentBase
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IHttpContextAccessor HttpContextAccessor { get; set; }
        [Inject]
        public ILocalStorageService LocalStorage { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            string code = null;
            StringValues codeParam;
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("code", out codeParam))
            {
                code = Convert.ToString(codeParam);
            }
            GetAzDoToken(code);
            NavigationManager.NavigateTo("/");

        }
        public void GetAzDoToken(string code)
        {
            LocalStorage.SetItemAsStringAsync("code", code);
            var nvc = new List<KeyValuePair<string, string>>();
            nvc.Add(new KeyValuePair<string, string>("assertion", code));
            nvc.Add(new KeyValuePair<string, string>("redirect_uri", "https://localhost:7195/authorize"));
            nvc.Add(new KeyValuePair<string, string>("client_assertion", "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im9PdmN6NU1fN3AtSGpJS2xGWHo5M3VfVjBabyJ9.eyJjaWQiOiJlOTU5NzI1Ny1iODY0LTRlMzQtYjNjZS01ZDYyZGY3N2U3ZGQiLCJjc2kiOiIyNTlkZWI0OS0wN2MxLTRlYjgtYjgzZS03NTM2NmI1ZmU5MmQiLCJuYW1laWQiOiI2ZTU3MzhhMS02MmZkLTY0MzUtOWQ1ZC0xNjM4YzcxNjI4ZmUiLCJpc3MiOiJhcHAudnN0b2tlbi52aXN1YWxzdHVkaW8uY29tIiwiYXVkIjoiYXBwLnZzdG9rZW4udmlzdWFsc3R1ZGlvLmNvbSIsIm5iZiI6MTY5NTEwMDg4NiwiZXhwIjoxODUyOTUzNjg2fQ.qKQ5NQ16j8F-WglNjYVto18B3C80D-VUxN6MqknULDt5cQELAALJ6JKPbOsd3oaGnqbUjXusOiX-OiidO1tWIcBElFsufzxdcyinbUIqXVmR_rihab3gzlg3fICKP-0D-vYSIV35JglEEHUW6Bq8p3bbufecQNsvK3jOz3_8TDmjUCodZBSHvKSKtqGmvnuqFzF5Ae3yQAEGasrEWxQAh__Y4ozJ4RGcYE6UqT2y5LnStJk4XuZYvYZFt9CYvxeJbZFKSMQ5_7IDeF6mCLuwCI3GntQDccjjkDS_ecR9J3A5Z_2fCv871OjZVfdQ9ndmCzK-RoO2vHJKLvTUkaVg3w"));
            nvc.Add(new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"));
            nvc.Add(new KeyValuePair<string, string>("client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"));
            try
            {
                using var client = new HttpClient();
                using var req = new HttpRequestMessage(HttpMethod.Post, "https://app.vssps.visualstudio.com/oauth2/token")
                {
                    Content = new FormUrlEncodedContent(nvc)
                };
                using var resp = client.SendAsync(req).Result;
                JObject response = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
                string responseBody = response.GetValue("access_token").ToString();

                LocalStorage.SetItemAsStringAsync("AzDoToken", responseBody);
                LocalStorage.SetItemAsStringAsync("RefreshToken", response.GetValue("refresh_token").ToString());
                LocalStorage.SetItemAsStringAsync("ExpiryTime", DateTime.UtcNow.AddMinutes(380).ToString()); //Expiry time is 1 hr after token issual (we do refresh token afetr 50mins)
         
            }
            catch (Exception)
            {

            }

        }
    }
}
