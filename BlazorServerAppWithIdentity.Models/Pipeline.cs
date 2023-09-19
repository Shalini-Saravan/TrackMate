using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorServerAppWithIdentity.Models
{
    public class Pipeline
    {
        public int Id { get; set; }
        public int Revision { get; set; }
        public string Name { get; set; }
        public string Folder { get; set; }
        
    }
}
