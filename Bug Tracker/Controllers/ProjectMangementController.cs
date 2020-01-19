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
        public async Task<IActionResult> Index(Project Project = null)
        {
            // Reset MongoDB 'Users' collection            
            await _userRepository.ResetUsers();
            Project = await _projectRepository.GetProject(Project.IDCode);
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

            // Sorting users assigned to selected project
            List<User> UsersAssignedList = new List<User>();
            List<User> UsersNotAssignedList = new List<User>();
            List<Issue> IssueList = new List<Issue>();


            // Model for view
            ProjectManagementViewModel model = new ProjectManagementViewModel();
            if (Project != null)
            {
                if (Project.IDCode != null)
                {
                    foreach (var user in AllUsers)
                    {
                        if (user.Projects.Contains(Project.IDCode))
                        {
                            UsersAssignedList.Add(user);
                        }
                        else
                        {
                            UsersNotAssignedList.Add(user);
                        }
                    }
                }

                if (Project.Issues == null)
                {
                    Project.Issues = IssueList;
                }


            }
            else
            {
                Project = new Project();
                Project.Issues = IssueList;
            }




            model.UserList = AllUsers;
            model.ProjectList = await _projectRepository.GetAllProjects(); 
            model.UsersAssignedList = UsersAssignedList;
            model.UsersNotAssignedList = UsersNotAssignedList;
            
            
            model.IssueList =IssueList;


            model.Project = Project;
            return View(model);
        }

        [HttpGet]
        public ActionResult CreateIssue()
        {
            return View("CreateIssue", new Issue());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateIssue([Bind(include: "ID, Name, Description, Project, Status, Submitter")] Issue issue)
        {
            if (ModelState.IsValid)
            {
                await _issueRepository.AddIssue(issue);
                TempData["Message"] = "User Createed Successfully";


            }

            var projectFromDb = await _projectRepository.GetProject(issue.Project);

            //Project projectIssueAdded = new Project();
            //projectIssueAdded = projectFromDb;
            if (projectFromDb.Issues == null)
            {
                projectFromDb.Issues = new List<Issue>();
            }

            projectFromDb.Issues.Add(issue);
            await _projectRepository.Update(projectFromDb);

            //Issue x = new Issue();
            //x = issue;
            //projectFromDb.Issues.Add(x);
            
            return RedirectToAction("Index", projectFromDb);
        }



        //[HttpGet]
        //[ActionName("Get")]
        public async Task<ActionResult> GetProjectById([Bind(include: "IDCode")] Project project)
        {
            var projectFromDb = await _projectRepository.GetProject(project.IDCode);
            if (projectFromDb == null)
            {
                return new NotFoundResult();
            }
            return RedirectToAction("Index", projectFromDb);
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


        public async Task<ActionResult> AddorRmove(string AddorRemove, Project project, User selectedUser, string Auth0ManagementAPI_AccessToken)
        {
            var userFromDb = await _userRepository.GetUser(selectedUser.ID);
            if (userFromDb == null)
            {
                return new NotFoundResult();
            }

            selectedUser.Id = userFromDb.Id;
            selectedUser.Projects = userFromDb.Projects;
            if (AddorRemove == "Add")
            {
                selectedUser.Projects.Add(project.IDCode);
            }
            else
            {
                selectedUser.Projects.Remove(project.IDCode);
            }

            object Projects = "{ \"projects\": [\"" + string.Join(",", selectedUser.Projects) + "\"]}}";
            if (AddorRemove == "Remove")
            {
                // If user has no more assigned projects, reset 'projects' metadata with '{}'
                if (selectedUser.Projects.Count == 0)
                {
                    Projects = "{ \"projects\": {}}}";
                }
            }

            // Use Auth0 API to add PROJECT to user metadata
            string authorizationValue = "Bearer " + Auth0ManagementAPI_AccessToken;
            string baseURL = "https://wussubininja.au.auth0.com/api/v2/users/" + selectedUser.ID;
            var client = new RestClient(baseURL);
            var request = new RestRequest(Method.PATCH);
            request.AddHeader("authorization", authorizationValue);
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", "{\"app_metadata\": " + Projects, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);


            //Issue issue = new Issue();
            //issue.ID = "test2";
            //issue.Name = "test2 name";
            //issue.Description = "test2 description";
            //await _issueRepository.AddIssue(issue);
            //project.Issues.Add(issue);
            
            return await GetProjectById(project);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update([Bind(include: "AddUsers, RemoveUsers, IDCode")] Project project)
        {
            if (ModelState.IsValid)
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


                // Adding users to project
                if (project.AddUsers != null)
                {
                    for (int i = 0; i < project.AddUsers.Count; i++)
                    {
                        User selectedUser = new User();
                        selectedUser.ID = project.AddUsers[i];

                        await AddorRmove("Add", project, selectedUser, Auth0ManagementAPI_AccessToken);
                    }
                }

                // Removing users from project
                if (project.RemoveUsers != null)
                {
                    for (int i = 0; i < project.RemoveUsers.Count; i++)
                    {
                        User selectedUser = new User();
                        selectedUser.ID = project.RemoveUsers[i];

                        await AddorRmove("Remove", project, selectedUser, Auth0ManagementAPI_AccessToken);
                    }
                }
            }

            //Issue issue = new Issue();
            //issue.ID = "test2";
            //issue.Name = "test2 name";
            //issue.Description = "test2 description";
            //await _issueRepository.AddIssue(issue);
            //project.Issues.Add(issue);

            return await GetProjectById(project);
            //return RedirectToAction("Index");
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