using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BugTrackerDataAccess.Models
{
    public class Role
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement]
        public string IDCode { get; set; }

        [BsonElement]
        public string Name { get; set; }

        [BsonElement]
        public string Description { get; set; }


    }
}