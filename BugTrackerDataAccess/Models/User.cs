using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BugTrackerDataAccess.Models
{
    public class User
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement]
   //     [Required]
        public int UserID { get; set; }

        [BsonElement]
    //    [Required]
        public string ID { get; set; }

        [BsonElement]
        //    [Required]
        public List<string> IDArray { get; set; }



        [BsonElement]
      //  [Required]
        public string UserName { get; set; }

        [BsonElement]
     //   [Required]
        public string Email { get; set; }

        [BsonElement]
     //   [Required]
        public string Role { get; set; }

        [BsonElement]
        //   [Required]
        public string RoleID { get; set; }

        [BsonElement]
        //[Required]
        public List<string> Projects { get; set; }

        [BsonElement]
        //[Required]
        public List<string> Issues { get; set; }


        [BsonElement]
   //     [Required]
        public int NumProjects { get; set; }



        //public string ProfileImage { get; set; }

    }








}