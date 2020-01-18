﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace BugTrackerDataAccess.Models
{
    public class Project
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement]
        public string ProjectID { get; set; }

        [BsonElement]
        public string ProjectName { get; set; }

        [BsonElement]
        public string ProjectDescription { get; set; }

        [BsonElement]
        public List<User> AssignedUsers { get; set; }

        [BsonElement]
        public List<Issue> Issues { get; set; }


    }
}