using BlazorServerAppWithIdentity.Api.Services;
using BlazorServerAppWithIdentity.Models;
using BlazorServerAppWithIdentity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace BlazorServerAppWithIdentity.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly SubscriptionService SubscriptionService;

        public SubscriptionController(SubscriptionService SubscriptionService)
        {
            this.SubscriptionService = SubscriptionService;
        }

        [HttpGet("{userName}")]
        public Subscription GetSubscriptionByUserName(string userName)
        {
            return SubscriptionService.GetSubscriptionByUserName(userName);
        }

        [HttpPost]
        public JsonResult UpdateSubscription([FromBody] string strdata)
        {
            JObject data = JObject.Parse(strdata);
            Subscription subscription = data["subscription"].ToObject<Subscription>();
            string userName = data["userName"].ToString();
            return new JsonResult(SubscriptionService.UpdateSubscription(subscription, userName).Result);
        }

    }
   
}
