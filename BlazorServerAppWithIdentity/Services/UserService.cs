using BlazorServerAppWithIdentity.Models;
using BlazorServerAppWithIdentity.Pages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace BlazorServerAppWithIdentity.Services
{
    public class UserService
    {
        private readonly HttpClient httpClient;
        private readonly IGlobalStateService GlobalState;
        public UserService(HttpClient httpClient, IGlobalStateService globalState)
        {
            this.httpClient = httpClient;
            GlobalState = globalState;
            this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalState.TokenValue);

        }

        public List<ApplicationRole> GetRoles()
        {
            return httpClient.GetFromJsonAsync<List<ApplicationRole>>("api/user/roles").Result;
        }
        public List<ApplicationUser> GetUsers()
        {   
            return httpClient.GetFromJsonAsync<List<ApplicationUser>>("api/user").Result;
        }
        public int GetUsersCount()
        {
            return httpClient.GetFromJsonAsync<int>("api/user/count").Result;
        }
        public ApplicationUser GetUserById(string id)
        {
            ApplicationUser User = httpClient.GetFromJsonAsync<ApplicationUser>("api/user/" + id).Result;
            return User;
        }

        public string AddUser(User user)
        {
            var jsonstring = "{'user' :" + JsonConvert.SerializeObject(user) + "}";
            var response = httpClient.PostAsJsonAsync<string>("api/user", jsonstring).Result;
            JObject responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            return responseJson["Result"].ToString();
            
        }

        public string AddPat(ApplicationUser user)
        {
            var jsonstring = "{\"user\" : " + JsonConvert.SerializeObject(user) + "}";
            var response = httpClient.PostAsJsonAsync<string>("api/user/pat", jsonstring).Result;
            if (response.IsSuccessStatusCode)
            {
                GlobalState.PAT = user.PAT;
            }
            JObject responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            return responseJson["Result"].ToString();
        }

        public string AddRole(ApplicationRole role)
        {
            var jsonstring = "{\"role\" : " + JsonConvert.SerializeObject(role) + "}";
            var response = httpClient.PostAsJsonAsync<string>("api/user/role", jsonstring).Result;
            
            JObject responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            return responseJson["Result"].ToString();
        }
        public string EditUser(User user, string uid)
        {
            var jsonstring = "{'user' :" + JsonConvert.SerializeObject(user) + ", \"uid\" : \"" + uid + "\"}";
            var response = httpClient.PutAsJsonAsync<string>("api/user", jsonstring).Result;
            JObject responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            return responseJson["Result"].ToString();

        }
        public string DeleteUser(ApplicationUser _user)
        {
            var jsonstring = "{'user' :" + JsonConvert.SerializeObject(_user) + "}";
            var response = httpClient.PostAsJsonAsync("api/user/delete", jsonstring).Result;
            JObject responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            return responseJson["Result"].ToString();
        }
    }
}
