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
using Microsoft.AspNetCore.Authorization;

namespace Bug_Tracker.Controllers
{

    public class UserManagementController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IIssueRepository _issueRepository;
        private readonly ProjectManagementController _projectManagementController;


        public UserManagementController(IUserRepository userRepository, IProjectRepository projectRepository, IRoleRepository roleRepository, IIssueRepository issueRepository, ProjectManagementController projectManagementController)
        {
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _roleRepository = roleRepository;
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
        public async Task<IActionResult> Index()
        {
            // Model for view
            UserManagementViewModel model = new UserManagementViewModel();
            List<Role> Roles = new List<Role>();
            List<User> Users = new List<User>();

            var allRoles = await _roleRepository.GetAllRoles();            
            var allUsers = await _userRepository.GetAllUsers();

            foreach (var role in allRoles)
            {
                if(role.Name != "Admin" && role.Name != "Project Manager")
                {
                    Roles.Add(role);
                }
            }

            foreach (var user in allUsers)
            {
                if (user.Role != "Admin" && user.Role != "Project Manager")
                {
                    Users.Add(user);
                }
            }


            // Assigning items to model
            model.RoleList = Roles;
            model.UserList = Users;


            return View(model);
        }



   

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update([Bind(include: "selectedUsersUpdate, selectedRole")] UserManagementViewModel model)
        {
            if (ModelState.IsValid)
            {

                foreach (var user in model.selectedUsersUpdate)
                {
                    // Updating MongoDB

                    User userFromDb = await _userRepository.GetUser(user);

                    userFromDb.Role = model.selectedRole;

                    await _userRepository.Update(userFromDb);

                    
                    // Updating Auth0

                    string baseURL = "https://wussubininja.au.auth0.com/api/v2/users/" + user;
                    string authorizationValue = "Bearer " + GetAuthorizationToken(); 
                    object newRole = "{ \"roles\": [\"" + model.selectedRole + "\"]}}";
                    var client = new RestClient(baseURL);
                    var request = new RestRequest(Method.PATCH);
                    request.AddHeader("authorization", authorizationValue);
                    request.AddHeader("content-type", "application/json");
                    request.AddParameter("application/json", "{\"app_metadata\": " + newRole, ParameterType.RequestBody);
                    IRestResponse response = client.Execute(request);


                }             
            }

            return RedirectToAction("Index");
        }


        // DELETE

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmation([Bind(include: "selectedUsersDelete")] UserManagementViewModel userManagementViewModel)
        {
            List<User> UserList = new List<User>();
            List<Issue> IssueList = new List<Issue>();

            foreach (var userSelected in userManagementViewModel.selectedUsersDelete)
            {
                var user = await _userRepository.GetUser(userSelected);
                         

                if (user.Projects != null)
                {

                    foreach (var project in user.Projects)
                    {

                        // REMOVING USER FROM PROJECT 
                        Project Project = await _projectRepository.GetProject(project.Split(':')[0]);

                        Project.RemoveUsers = new List<string>();
                        Project.RemoveUsers.Add(userSelected);
                        Project.AddUsers = null;

                        await _projectRepository.Update(Project);

                        await _projectManagementController.UpdateProjectAssignment(Project);

                        // REMOVING USER FROM ASSOCIATED ISSUES FROM PROJECT

                        if (user.Issues != null)
                        {
                            foreach (var issue in user.Issues)
                            {
                                Issue Issue = await _issueRepository.GetIssue(issue.Split('-')[1].Split(':')[0]);
                                Issue.RemoveUsers = new List<string>();
                                Issue.RemoveUsers.Add(userSelected);
                                Issue.AddUsers = null;
                                await _issueRepository.Update(Issue);

                                await _projectManagementController.UpdateIssue(Issue);
                            }
                        }
       
                    }
                }

                UserList.Add(user);

                // Deleting user from Auth0
                string baseURL = "https://wussubininja.au.auth0.com/api/v2/users/" + userSelected;
                var client = new RestClient(baseURL);
                var request = new RestRequest(Method.DELETE);
                request.AddHeader("authorization", "Bearer " +  GetAuthorizationToken());
                IRestResponse response = client.Execute(request);
            }


            var result = await _userRepository.Delete(UserList);


            if (result)
            {
                TempData["Message"] = "User Deleted Successfully";
            }
            else
            {
                TempData["Message"] = "Error While Deleting the User";
            }

            return RedirectToAction("Index");
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult> DeleteUsers()
        {
            UserManagementViewModel model = new UserManagementViewModel();
            model.UserList = await _userRepository.GetAllUsers();


            return View("DeleteUsers", model);
        }


        public async Task<ActionResult> DeleteUsers([Bind(include: "selectedUsersDelete")] UserManagementViewModel userManagementViewModel)
        {
            UserManagementViewModel model = new UserManagementViewModel();
            List<User> UserList = new List<User>();


            foreach (var userSelected in userManagementViewModel.selectedUsersDelete)
            {
                var user = await _userRepository.GetUser(userSelected);


                UserList.Add(user);
            }

            model = userManagementViewModel;
            model.UserList = UserList;

            return View("DeleteConfirmation", model);
        }









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