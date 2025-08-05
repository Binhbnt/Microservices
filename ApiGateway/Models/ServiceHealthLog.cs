using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ApiGateway.Models
{
    public class ServiceHealthLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string ServiceName { get; set; }
        public string Status { get; set; }  // Healthy, Unreachable, Error
        public DateTime CheckedAt { get; set; }
    }
}
