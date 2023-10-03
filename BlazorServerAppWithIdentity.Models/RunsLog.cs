using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorServerAppWithIdentity.Models
{
    public class RunsLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string RunId { get; set; }
        public string PipelineId { get; set; }
        public string UserName { get; set; }
    }
}
