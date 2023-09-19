using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorServerAppWithIdentity.Models
{
    public class Run
    {
        public string id { get; set; }
        public string name { get; set; }
        public string state { get; set; }
        public string result { get; set; }
        public DateTime createdDate { get; set; }
        public DateTime finishedDate { get; set; }

        public Pipeline pipeline { get; set; }

    }
}
