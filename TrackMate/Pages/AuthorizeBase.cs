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
            nvc.Add(new KeyValuePair<string, string>("client_assertion", "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im9PdmN6NU1fN3AtSGpJS2xGWHo5M3VfVjBabyJ9.eyJjaWQiOiJhNGEzZTA1OS0xMDgzLTRjZmUtYmNkYy0xODY5NmY5YTcxNmMiLCJjc2kiOiIwNzM5ZmE3OC1lMzk0LTQwYTEtODQ1Ny1hZWU2YmU1MDVjOWUiLCJuYW1laWQiOiI2ZTU3MzhhMS02MmZkLTY0MzUtOWQ1ZC0xNjM4YzcxNjI4ZmUiLCJpc3MiOiJhcHAudnN0b2tlbi52aXN1YWxzdHVkaW8uY29tIiwiYXVkIjoiYXBwLnZzdG9rZW4udmlzdWFsc3R1ZGlvLmNvbSIsIm5iZiI6MTY5ODIwODgyMSwiZXhwIjoxODU2MDYxNjIxfQ.I_14AciVtAf_DLKIjWqofLWzqsTIattsQViIWj4vB0lnrZp_noAEVxK7KlA83xzOaHaw9ABFdBUYc4ekr-2VRZeISk37XF4AFql4JQ9VSdntxkbmuBlMpi5MV1bt-gqcI918Uth5JSjUKGjWxl9dqk2QPij9l2YXsU9AauWfarC-Bf-lmtg7xjjXqllUMgvfu1LrUTQZ4WkR8ya8TNTi8xrA6e2crnMx3nE3GDz_wfv3MhSkR7MYuxZmES_z42Rre2PJI7ERUtO4GykU9cFlznfzas-V4ccW-NTFQpG9GStM1KcPWT6hnCmUdl2mfmH7VMnu-yxFha-OI6mRPNjnxQ"));
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
