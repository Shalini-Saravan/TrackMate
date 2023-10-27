using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TrackMate.Api.Services;
using TrackMate.Models;

namespace TrackMate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RunsLogController : ControllerBase
    {
        private readonly RunsLogService RunsLogService;
        public RunsLogController(RunsLogService RunsLogService)
        {
            this.RunsLogService = RunsLogService;
        }

        [HttpPost]
        public JsonResult AddRunsLog([FromBody] string strdata)
        {
            JObject data = JObject.Parse(strdata);
            RunsLog log = data["runsLog"].ToObject<RunsLog>();
            if (log != null)
            {
                return new JsonResult(RunsLogService.AddRunsLog(log));
            }
            return null;
        }
    }
}
