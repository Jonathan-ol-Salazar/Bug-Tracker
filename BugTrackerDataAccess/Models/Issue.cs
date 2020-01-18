using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace BugTrackerDataAccess.Models
{
    public class Issue
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement]
        public string ID { get; set; }

        [BsonElement]
        public string Name { get; set; }

        // EMUM ??
        [BsonElement]
        public string Status { get; set; }

        [BsonElement]
        public string Submitter { get; set; }


        [BsonElement]
        public string Description { get; set; }

        [BsonElement]
        public List<User> AssignedUsers { get; set; }

        [BsonElement]
        public int NumUsers { get; set; }


        [BsonElement]
        public string DateCreated { get; set; }

        [BsonElement]
        public string LastUpdated { get; set; }



        // ADD IMAGE
        //[BsonElement]
        //public string Tickets { get; set; }


        // ADD MORE STUFF

    }
}