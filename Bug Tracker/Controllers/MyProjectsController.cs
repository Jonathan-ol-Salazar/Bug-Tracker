using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BugTrackerDataAccess.Models;
using BugTrackerDataAccess.Repositories;
using Bug_Tracker.Models;
using RestSharp;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections;
using BugTrackerDataAccess.ViewModel;
using System;
using System.Linq;
using System.Security.Claims;
using Nancy.Json;

namespace Bug_Tracker.Controllers
{
    public class MyProjectsController : Controller
    {

        private readonly IUserRepository _userRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IIssueRepository _issueRepository;

        public MyProjectsController(IUserRepository userRepository, IProjectRepository projectRepository, IIssueRepository issueRepository)
        {
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _issueRepository = issueRepository;

        }


        [HttpGet]
        public async Task<IActionResult> Index(Project Project = null)
        {
            // Sorting users assigned to selected project
            List<User> UsersAssignedList = new List<User>();
            List<User> UsersNotAssignedList = new List<User>();
            List<Issue> IssueList = new List<Issue>();
            List<Project> ProjectList = new List<Project>();


            // Model for view
            MyProjectsViewModel model = new MyProjectsViewModel();
            await _userRepository.ResetUsers();


            // ACCESS TOKEN FOR AUTH0 MANAGEMENT API
            var client = new RestClient("https://wussubininja.au.auth0.com/oauth/token");
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", "grant_type=client_credentials&client_id=LZ1ZnJCpRTSZB4b2iET97KhOajNiPyLk&client_secret=6Actr7Xa1tNRC6370iM6rzD68Wbpq8UCurK3QbtBiRRAUZqheOwFzDspQkZ2-7QJ&audience=https://wussubininja.au.auth0.com/api/v2/", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            // Parsing into JSON 
            var response2dict = JObject.Parse(response.Content);
            // Retrieving Access Token
            var Auth0ManagementAPI_AccessToken = response2dict.First.First.ToString();

            // get user 
            // get user metadata
            // use user metadata to get projects from mongodb
            // add projects to list 
            // add projects to model 

            // GETTING AUTH0 USER DETAILS OF CURRENT SIGNED IN USER
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            string baseURL = "https://wussubininja.au.auth0.com/api/v2/users/" + userId;
            client = new RestClient(baseURL);
            request = new RestRequest(Method.GET);
            request.AddHeader("authorization", "Bearer " + Auth0ManagementAPI_AccessToken);
            response = client.Execute(request);

            // Parsing into dynamic variable
            var projectDict = new JavaScriptSerializer().Deserialize<dynamic>(response.Content);
            // Dictionary of current users projects
            var projects = projectDict["app_metadata"]["projects"];

            var serializer = new JavaScriptSerializer(); //using System.Web.Script.Serialization;

            Dictionary<string, string> values = serializer.Deserialize<Dictionary<string, string>>(response.Content);


            //Dictionary<string, object> yeet = new JavaScriptSerializer().Deserialize<dynamic>(response.Content);

            foreach (KeyValuePair<string, object> project in projects)
            {
                ProjectList.Add(await _projectRepository.GetProject(project.Key)); 
                               
            }



            model.ProjectList = ProjectList;
            return View(model);
        }





    }
}