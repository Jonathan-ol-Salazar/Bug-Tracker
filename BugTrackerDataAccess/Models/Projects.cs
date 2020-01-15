using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace BugTrackerDataAccess.Auth0.Models
{
    public class Projects
    {
        [BsonElement]
        public string RoleDescription { get; set; }


    }
}