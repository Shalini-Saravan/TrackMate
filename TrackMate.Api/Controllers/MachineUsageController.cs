using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrackMate.Api.Services;
using TrackMate.Models;

namespace TrackMate.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MachineUsageController : ControllerBase
    {
        private readonly MachineUsageService MachineUsageService;

        public MachineUsageController(MachineUsageService machineUsageService)
        {
            MachineUsageService = machineUsageService;
        }

        [HttpGet("{userName}")]
        public IEnumerable<MachineUsage> GetByUserName(string userName)
        {
            return MachineUsageService.GetMachineUsageByUserName(userName);
        }
    }
}
