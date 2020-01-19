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
   //     [Required]
        public int NumProjects { get; set; }



        //public string ProfileImage { get; set; }



        //public void AddorRmove(string AddorRemove, Project project, User selectedUser)
        //{
        //    var userFromDb = await _userRepository.GetUser(selectedUser.ID);
        //    if (userFromDb == null)
        //    {
        //        return new NotFoundResult();
        //    }

        //    selectedUser.Id = userFromDb.Id;
        //    selectedUser.Projects = userFromDb.Projects;
        //    if (AddorRemove == "Add")
        //    {
        //        selectedUser.Projects.Add(project.IDCode);
        //    }
        //    else
        //    {
        //        selectedUser.Projects.Remove(project.IDCode);
        //    }

        //    object Projects = "{ \"projects\": [\"" + string.Join(",", selectedUser.Projects) + "\"]}}";
        //    if (AddorRemove == "Remove")
        //    {
        //        if (selectedUser.Projects.Count == 0)
        //        {
        //            Projects = "{ \"projects\": {}}}";
        //        }
        //    }


        //    // Use Auth0 API to add PROJECT to user metadata
        //    string authorizationValue = "Bearer " + Auth0ManagementAPI_AccessToken;
        //    string baseURL = "https://wussubininja.au.auth0.com/api/v2/users/" + selectedUser.ID;
        //    client = new RestClient(baseURL);
        //    request = new RestRequest(Method.PATCH);
        //    request.AddHeader("authorization", authorizationValue);
        //    request.AddHeader("content-type", "application/json");
        //    request.AddParameter("application/json", "{\"app_metadata\": " + Projects, ParameterType.RequestBody);
        //    response = client.Execute(request);


        //}












    }








}