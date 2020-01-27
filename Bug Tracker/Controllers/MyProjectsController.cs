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
        private readonly ProjectManagementController _projectManagementController;


        public MyProjectsController(IUserRepository userRepository, IProjectRepository projectRepository, IIssueRepository issueRepository, ProjectManagementController projectManagementController)
        {
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _issueRepository = issueRepository;
            _projectManagementController = projectManagementController;

        }
        public string GetAuthorizationToken()
        {
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

            return Auth0ManagementAPI_AccessToken;
        }

        [HttpGet]
        public async Task<IActionResult> Index(Project Project = null)
        {

            // GETTING AUTH0 USER DETAILS OF CURRENT SIGNED IN USER
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;


            List<Project> ProjectList = new List<Project>();

            // Model for view
            MyProjectsViewModel model = new MyProjectsViewModel();


            var currentUser = await _userRepository.GetUser(userId);




            foreach (var project in currentUser.Projects)
            {
                string projectIDCode = project.Split(':')[0].Replace("\"", "");
                //projectIDCode = projectIDCode.Replace("\"", "");

                ProjectList.Add(await _projectRepository.GetProject(projectIDCode));


            }


            if (model.ProjectList == null)
            {
                model.ProjectList = new List<Project>();

            }

            model.ProjectList = ProjectList;
            return View(model);


        }


        [HttpGet]
        public async Task<ActionResult> CreateProject()
        {
            MyProjectsViewModel model = new MyProjectsViewModel();

            Project project = new Project();
            List<User> ProjectManagerList = new List<User>();

            var AllUsers = await _userRepository.GetAllUsers();

            foreach (var user in AllUsers)
            {
                if (user.Role == "Project Manager")
                {
                    ProjectManagerList.Add(user);
                }
            }


            model.ProjectManagerList = ProjectManagerList;


            model.UserList = AllUsers;
            model.Project = project;

            return View("CreateProject", model);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateProject([Bind(include: "IDCode, Name, Description, ProjectManagerUserID, AddUsers")] Project project)
        {
            if (ModelState.IsValid)
            {
                var userFromDb = await _userRepository.GetUser(project.ProjectManagerUserID);

                project.ProjectManagerUserID = userFromDb.ID;
                project.ProjectManagerUserName = userFromDb.UserName;

                project.Created = DateTime.UtcNow.ToString();
                project.Issues = new List<string>();
                project.Updated = project.Created;



                await _projectRepository.AddProject(project);


                await _projectManagementController.UpdateProjectAssignment(project);



                TempData["Message"] = "User Createed Successfully";
            }
            return RedirectToAction("Index");
        }



    }
}