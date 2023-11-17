using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackMate.Models
{
    public class Subscription
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserName { get; set; }
        public bool Machine_CheckIns_CheckOuts { get; set; } = false;
        public bool Pipeline_Build_Completion { get; set; } = false;
        public bool Email_TimeOut_Notification {  get; set; } = false;
    }
}
