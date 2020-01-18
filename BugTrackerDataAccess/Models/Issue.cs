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
        public string IssueID { get; set; }

        [BsonElement]
        public string IssueName { get; set; }

        [BsonElement]
        public string IssueDescription { get; set; }

        [BsonElement]
        public List<User> AssignedUsers { get; set; }

        // ADD IMAGE
        //[BsonElement]
        //public string Tickets { get; set; }


        // ADD MORE STUFF

    }
}