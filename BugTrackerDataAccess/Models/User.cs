using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;



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

        [BsonElement]
        //     [Required]
        public string DOB { get; set; }

        [BsonElement]
        //     [Required]
        public string About { get; set; }

        [BsonElement]
        //     [Required]
        public string Location { get; set; }

        [BsonElement]
        //     [Required]
        public string Skills { get; set; }

        [BsonElement]
        //     [Required]
        public string Education { get; set; }


        [BsonElement]

        public List<string> User_Metadata { get; set; }

        [BsonElement]

        public string AccountImageDefault { get; set; }


        [BsonElement]

        public string AccountImageString { get; set; }

        [BsonElement]

        public byte[] AccountImageArray { get; set; }





    }








}