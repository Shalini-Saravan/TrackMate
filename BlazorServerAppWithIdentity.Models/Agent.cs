using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorServerAppWithIdentity.Models
{
    public class Agent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Status { get; set; }
        public Request AssignedRequest { get; set; }
        public string OSDescription { get; set; }

        [JsonProperty("systemCapabilities")]
        public AgentCapability Capability { get; set; }
    }
}
