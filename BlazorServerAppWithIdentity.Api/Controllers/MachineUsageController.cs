using BlazorServerAppWithIdentity.Models;
using BlazorServerAppWithIdentity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorServerAppWithIdentity.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(AuthenticationSchemes = "Bearer")]
    public class MachineUsageController : ControllerBase
    {
        private readonly MachineUsageService MachineUsageService;

        public MachineUsageController(MachineUsageService machineUsageService)
        {
            this.MachineUsageService = machineUsageService;
        }

        [HttpGet("{userName}")]
        public IEnumerable<MachineUsage> GetByUserName(string userName)
        {
            return MachineUsageService.GetMachineUsageByUserName(userName);
        }
    }
}
