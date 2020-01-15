using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BugTrackerDataAccess.Models
{
    public class Roles
    {
        [BsonElement]
        public string Role { get; set; }

        [BsonElement]
        public string RoleID { get; set; }

        [BsonElement]
        public string RoleDescription { get; set; }


    }
}