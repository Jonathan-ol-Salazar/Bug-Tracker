using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace BugTrackerDataAccess.Models
{
    public class Project
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement]
        public string IDCode { get; set; }

        [BsonElement]
        public string Name { get; set; }

        [BsonElement]
        public string Description { get; set; }

        [BsonElement]
        public List<User> AssignedUsers { get; set; }

        [BsonElement]
        public List<Issue> Issues { get; set; }

        [BsonElement]
        public List<string> AddUsers { get; set; }

        [BsonElement]
        public List<string> RemoveUsers { get; set; }

        [BsonElement]
        public User ProjectManager { get; set; }

        [BsonElement]
        public string DateCreated { get; set; }

        [BsonElement]
        public string LastUpdated { get; set; }



    }
}