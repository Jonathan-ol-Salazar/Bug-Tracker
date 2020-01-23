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
        public string IDCode { get; set; }

        [BsonElement]
        public string Title { get; set; }

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
        public List<string> Users { get; set; }


        //[BsonElement]
        //public int NumUsers { get; set; }

        public List<string> AddUsers { get; set; }

        [BsonElement]
        public string Created { get; set; }

        [BsonElement]
        public string Updated { get; set; }

        [BsonElement]
        public string ProjectIDCode { get; set; }


        // ADD IMAGE
        //[BsonElement]
        //public string Tickets { get; set; }


        // ADD MORE STUFF

    }
}