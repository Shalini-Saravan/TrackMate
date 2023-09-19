using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace BlazorServerAppWithIdentity.Models
{
    public class AgentCapability
    {
        [JsonProperty("Agent.Name")]
        public string AgentName { get; set; }

        [JsonProperty("Agent.Version")]
        public string AgentVersion { get; set; }

        [JsonProperty("Agent.ComputerName")]
        public string AgentComputerName { get; set; }

        [JsonProperty("Agent.HomeDirectory")]
        public string AgentHomeDirectory { get; set; }

        [JsonProperty("Agent.OS")]
        public string AgentOS { get; set; }

        [JsonProperty("Agent.OSArchitecture")]
        public string AgentOSArchitecture { get; set; }

        [JsonProperty("Agent.OSVersion")]
        public string AgentOSVersion { get; set; }
        public string AGENT_POOL_NAME { get; set; }
        public string ALLUSERSPROFILE { get; set; }
        public string APPDATA { get; set; }
        public string ChocolateyInstall { get; set; }
        public string Cmd { get; set; }
        public string CommonProgramFiles { get; set; }

        [JsonProperty("CommonProgramFiles(x86)")]
        public string CommonProgramFilesx86 { get; set; }
        public string CommonProgramW6432 { get; set; }
        public string COMPUTERNAME { get; set; }
        public string ComSpec { get; set; }
        public string DotNetFramework { get; set; }

        [JsonProperty("DotNetFramework_4.8.0")]
        public string DotNetFramework_480 { get; set; }

        [JsonProperty("DotNetFramework_4.8.0_x64")]
        public string DotNetFramework_480_x64 { get; set; }
        public string DriverData { get; set; }
        public string FPS_BROWSER_APP_PROFILE_STRING { get; set; }
        public string FPS_BROWSER_USER_PROFILE_STRING { get; set; }
        public string HOMEDRIVE { get; set; }
        public string HOMEPATH { get; set; }
        public string InteractiveSession { get; set; }
        public string LOCALAPPDATA { get; set; }
        public string LOGONSERVER { get; set; }
        public string Machine_Purpose { get; set; }
        public string MSBuild { get; set; }

        [JsonProperty("MSBuild_17.0")]
        public string MSBuild_170 { get; set; }

        [JsonProperty("MSBuild_17.0_x64")]
        public string MSBuild_170_x64 { get; set; }

        [JsonProperty("MSBuild_4.0")]
        public string MSBuild_40 { get; set; }

        [JsonProperty("MSBuild_4.0_x64")]
        public string MSBuild_40_x64 { get; set; }
        public string MSBuild_x64 { get; set; }
        public string NIEXTCCOMPILERSUPP { get; set; }
        public string NUMBER_OF_PROCESSORS { get; set; }
        public string OneDrive { get; set; }
        public string OS { get; set; }
        public string Path { get; set; }
        public string PATHEXT { get; set; }
        public string PowerShell { get; set; }
        public string PROCESSOR_ARCHITECTURE { get; set; }
        public string PROCESSOR_IDENTIFIER { get; set; }
        public string PROCESSOR_LEVEL { get; set; }
        public string PROCESSOR_REVISION { get; set; }
        public string ProgramData { get; set; }
        public string ProgramFiles { get; set; }

        [JsonProperty("ProgramFiles(x86)")]
        public string ProgramFilesx86 { get; set; }
        public string ProgramW6432 { get; set; }
        public string PROMPT { get; set; }
        public string PUBLIC { get; set; }
        public string PYTHONIOENCODING { get; set; }
        public string RUBYOPT { get; set; }
        public string SESSIONNAME { get; set; }
        public string SystemDrive { get; set; }
        public string SystemRoot { get; set; }
        public string TEMP { get; set; }
        public string TestStand { get; set; }
        public string TestStand64 { get; set; }
        public string TestStandAppData { get; set; }
        public string TestStandAppData64 { get; set; }
        public string TestStandBin { get; set; }
        public string TestStandBin64 { get; set; }
        public string TestStandPublic { get; set; }
        public string TestStandPublic64 { get; set; }
        public string TMP { get; set; }
        public string TS_KeyFile { get; set; }
        public string TS_Password { get; set; }
        public string TS_User { get; set; }
        public string USERDOMAIN { get; set; }
        public string USERDOMAIN_ROAMINGPROFILE { get; set; }
        public string USERNAME { get; set; }
        public string USERPROFILE { get; set; }
        public string VERBOSE_ARG { get; set; }
        public string VisualStudio { get; set; }

        [JsonProperty("VisualStudio_17.0")]
        public string VisualStudio_170 { get; set; }
        public string VisualStudio_IDE { get; set; }

        [JsonProperty("VisualStudio_IDE_17.0")]
        public string VisualStudio_IDE_170 { get; set; }
        public string VS140COMNTOOLS { get; set; }
        public string VSTest { get; set; }

        [JsonProperty("VSTest_17.0")]
        public string VSTest_170 { get; set; }
        public string Windir { get; set; }
        public string WindowsSdk { get; set; }

        [JsonProperty("WindowsSdk_10.0")]
        public string WindowsSdk_100 { get; set; }

    }
}
