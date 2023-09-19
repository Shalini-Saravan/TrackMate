using BlazorServerAppWithIdentity.Models;
using BlazorServerAppWithIdentity.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;

namespace BlazorServerAppWithIdentity.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(AuthenticationSchemes = "Bearer")]
    public class MachineController : ControllerBase
    {

        private readonly MachineService MachineService;

        public MachineController(MachineService machineService)
        {
            MachineService = machineService;
        }
        

        [HttpGet]
        [AllowAnonymous]
        public Task<IEnumerable<Machine>> GetAll()
        {
            return MachineService.GetMachines();
        }

        [HttpGet("agents")]
        [AllowAnonymous]
        public Task<IEnumerable<Machine>> GetAgents()
        {
            return MachineService.GetAgents();
        }

        [HttpGet("agent/{agentId}")]
        public Task<Machine> GetMachineByAgentId(string agentId)
        {
            return MachineService.GetMachineByAgentId(agentId);
        }

        [HttpGet("agents/reserved/{userName}")]
        public Task<IEnumerable<Machine>> GetReservedAgents(string userName)
        {
            return MachineService.GetReservedAgents(userName);
        }

        [HttpGet("count")]
        public Task<int> GetCount()
        {
            return MachineService.GetMachineCount();
        }

        [HttpGet("available/count")]
        public Task<int> GetAvailableCount()
        {
            return MachineService.GetAvailableMachineCount();
        }

        
        [HttpGet("{id}")]
        public Machine GetById(string id) 
        {
            return MachineService.GetMachineById(id);
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult AddMachine([FromBody] string strdata) 
        {
            JObject data = JObject.Parse(strdata);
            Machine machine = data["machine"].ToObject<Machine>();
            return new JsonResult(MachineService.AddMachine(machine));
        }

        [HttpPut]
        public JsonResult UpdateMachine([FromBody] string strdata)
        {
            JObject data = JObject.Parse(strdata);
            Machine machine = data["machine"].ToObject<Machine>();

            return new JsonResult(MachineService.UpdateMachine(machine));
        }

        [HttpDelete("{id}")]
        [AllowAnonymous]
        public JsonResult DeleteMachine(string id)
        {
            return new JsonResult(MachineService.RemoveMachine(id));
        }

        [HttpPost("assign")]
        public JsonResult AssignMachine([FromBody] string strdata)
        {
            JObject data = JObject.Parse(strdata);

            Machine machine = data["machine"].ToObject<Machine>();
            string userId = data["userId"].ToString();
            string userName = data["userName"].ToString();
            string comments = data["comments"].ToString();
            DateTime endTime = Convert.ToDateTime(data["endTime"].ToString()) ;

            return new JsonResult(MachineService.AssignUser(userId, userName, machine, endTime, comments));
          
        }

        [HttpPut("revoke")]
        public JsonResult RevokeMachine([FromBody] string strdata)
        {
            JObject data = JObject.Parse(strdata);
            Machine machine = data["machine"].ToObject<Machine>();
            
            return new JsonResult(MachineService.RevokeUser(machine));
        }

        [HttpGet("user/{id}")]
        public List<Machine> GetByUserId(string id)
        {
            return MachineService.GetMachinesByUserId(id).Result;
        }
    }
}

