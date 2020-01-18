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

namespace Bug_Tracker.Controllers
{

    public class ProjectManagementController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IIssueRepository _issueRepository;

        public ProjectManagementController(IUserRepository userRepository, IProjectRepository projectRepository, IIssueRepository issueRepository)
        {
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _issueRepository = issueRepository;

        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Reset MongoDB 'Users' collection
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
            


            // GETTING ALL USERS
            string baseURL = "https://wussubininja.au.auth0.com/api/v2/users";
            string authorizationValue = "Bearer " + Auth0ManagementAPI_AccessToken;
            // Endpoint to get user role
            client = new RestClient(baseURL);
            request = new RestRequest(Method.GET);
            // Add Auth0 Management API Access Token 
            request.AddHeader("authorization", authorizationValue);
            response = client.Execute(request);

            JArray usersAuth0 = JArray.Parse(response.Content);
            var AllUsers = new List<User>();

            foreach (var userAuth0 in usersAuth0)
            {
                // ArrayList to store all user assigned projects
                List<string> Projects = new List<string>();
                //var Projects = new ArrayList();
                if (userAuth0.SelectToken("app_metadata").SelectToken("projects").First != null)
                {
                    // Foreach loop to add all projects for Projects ArrayList
                    foreach (var project in userAuth0.SelectToken("app_metadata").SelectToken("projects"))
                    {
                        //project.SelectToken("app_metadata").SelectToken("projects");
                        Projects.Add(project.ToString());
                    }
                }

                // GETTING ROLES ASSIGNED TO USER FROM AUTH0
                baseURL = "https://wussubininja.au.auth0.com/api/v2/users/" + userAuth0.SelectToken("user_id").ToString() + "/roles";
                client = new RestClient(baseURL);
                response = client.Execute(request);
                JArray userRolesAuth0 = JArray.Parse(response.Content);


                var document = new User();

                document.ID = userAuth0.SelectToken("user_id").ToString();
                document.UserName = userAuth0.SelectToken("name").ToString();
                document.Email = userAuth0.SelectToken("email").ToString();
                document.Role = userRolesAuth0.First.SelectToken("name").ToString();
                document.RoleID = userRolesAuth0.First.SelectToken("id").ToString();
                document.Projects = Projects;
                document.NumProjects = Projects.Count;


                AllUsers.Add(document);
            }           

            // Add all users from Auth0 to MongoDB 'Users' collection
            await _userRepository.AddUsers(AllUsers);
            // Get all users from 'Users' collection and use a model for 'Index' view
            // var GetAllUsers = await _userRepository.GetAllUsers();
            // Get all projects from 'Projects' collection and use a model for 'Index' view
            var GetAllProjects = await _projectRepository.GetAllProjects();
            var GetAllIssues = await _issueRepository.GetAllIssues();

            // Model for view
            ProjectManagementViewModel model = new ProjectManagementViewModel();
            model.UserList = AllUsers;
            model.ProjectList = GetAllProjects;
            model.IssueList = GetAllIssues;
            return View(model);
        }

        [HttpGet]
        //[ActionName("Get")]
        public async Task<ActionResult> GetUserById(string id)
        {
            var user = await _userRepository.GetUser(id);
            if (user == null)
            {
                return new NotFoundResult();
            }
            return View("GetUserById", user);
        }


        [HttpGet]
        public ActionResult Create()
        {
            return View("Create", new User());
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(include: "UserID, UserName, Email, Role")] User user)
        {
            if (ModelState.IsValid)
            {
                await _userRepository.Create(user);
                TempData["Message"] = "User Createed Successfully";

            }
            return RedirectToAction("Index");
        }


        //[HttpGet]
        //public async Task<ActionResult> Update(string id)
        //{
        //    User user = await _userRepository.GetUser(id);
        //    return View("Update", user);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update([Bind(include: "IDArray, RoleID, Projects")] User user)
        {
            if (ModelState.IsValid)
            {
                for (int i = 0; i < user.IDArray.Count; i++)  //foreach (var selectedUserID in selectedUsers.)
                {
                    User selectedUser = new User();
                    selectedUser.ID = user.IDArray[i];
                    selectedUser.Projects = user.Projects;
                    selectedUser.RoleID = user.RoleID;




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


                    var userFromDb = await _userRepository.GetUser(selectedUser.ID);
                    if (userFromDb == null)
                    {
                        return new NotFoundResult();
                    }
                    selectedUser.Id = userFromDb.Id;

                    //await _userRepository.Update(selectedUser);
                    TempData["Message"] = "Customer Updated Successfully";

                    string baseURL = "";
                    string authorizationValue = "Bearer " + Auth0ManagementAPI_AccessToken;
                    if (selectedUser.RoleID != null)
                    {
                        //Use Auth0 API to remove all users ROLES
                        baseURL = "https://wussubininja.au.auth0.com/api/v2/users/" + selectedUser.ID + "/roles";
                        object oldRole = "{ \"roles\": [ \"" + userFromDb.RoleID + "\"] }";
                        client = new RestClient(baseURL);
                        request = new RestRequest(Method.DELETE);
                        request.AddHeader("content-type", "application/json");
                        request.AddHeader("authorization", authorizationValue);
                        request.AddHeader("cache-control", "no-cache");
                        request.AddParameter("application/json", oldRole, ParameterType.RequestBody);
                        response = client.Execute(request);

                        // Use Auth0 API to add ROLE to user
                        object newRole = "{ \"roles\": [ \"" + selectedUser.RoleID + "\"] }";
                        request = new RestRequest(Method.POST);
                        request.AddHeader("content-type", "application/json");
                        request.AddHeader("authorization", authorizationValue);
                        request.AddHeader("cache-control", "no-cache");
                        request.AddParameter("application/json", newRole, ParameterType.RequestBody);
                        response = client.Execute(request);
                    }

                    // Use Auth0 API to add PROJECT to user metadata
                    baseURL = "https://wussubininja.au.auth0.com/api/v2/users/" + selectedUser.ID;
                    object newProject = "{\"projects\": {}}}";

                    if (selectedUser.Projects != null)
                    {
                        newProject = "{ \"projects\": [\"" + string.Join(",", selectedUser.Projects) + "\"]}}";
                    }

                    client = new RestClient(baseURL);
                    request = new RestRequest(Method.PATCH);
                    request.AddHeader("authorization", authorizationValue);
                    request.AddHeader("content-type", "application/json");
                    request.AddParameter("application/json", "{\"app_metadata\": " + newProject, ParameterType.RequestBody);
                    response = client.Execute(request);

                }

            }


            return RedirectToAction("Index");



        }

        public async Task<ActionResult> ConfirmDelete(string id)
        {
            var userFromDb = await _userRepository.GetUser(id);
            return View("ConfirmDelete", userFromDb);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Delete(int id)
        //{
        //    var user = await _userRepository.GetUser(id);
        //    if (user == null)
        //    {
        //        Console.WriteLine("OMFL");
        //        return new NotFoundResult();

        //    }
        //    var result = await _userRepository.Delete(user.ID);
        //    if (result)
        //    {
        //        TempData["Message"] = "User Deleted Successfully";
        //    }
        //    else
        //    {
        //        TempData["Message"] = "Error While Deleting the User";
        //    }
        //    return RedirectToAction("Index");
        //}


        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }






}