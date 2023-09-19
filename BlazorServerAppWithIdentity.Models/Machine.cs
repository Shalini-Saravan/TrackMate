using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BlazorServerAppWithIdentity.Models
{
    public class Machine
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string AgentId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public string RAMGB { get; set; }
        public string Cores { get; set; }
        public string Status { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }

        public DateTime? FromTime { get; set; }
        public DateTime? EndTime { get; set; }

        public DateTime? LastAccessed { get; set; }
        public string Purpose { get; set; }
        public string Comments { get; set; }


    }
}
