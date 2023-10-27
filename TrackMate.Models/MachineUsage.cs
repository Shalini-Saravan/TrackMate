using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace TrackMate.Models
{
    public class MachineUsage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public Machine Machine { get; set; }
        public string UserName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public bool InUse { get; set; }


    }
}
