﻿using System.Diagnostics;
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



            var allRoles = await _roleRepository.GetAllRoles();
            var allUsers = await _userRepository.GetAllUsers();
            //var allProjects = await _projectRepository.GetAllProjects();

            // Assigning items to model
            model.UserList = allUsers;
            model.RoleList = allRoles;
            //model.ProjectList = allProjects;

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
                    //request.AddParameter("application/json", "{\"user_metadata\": {\"addresses\": {\"home\": \"123 Main Street, Anytown, ST 12345\"}}}", ParameterType.RequestBody);
                    request.AddParameter("application/json", "{\"app_metadata\": " + newRole, ParameterType.RequestBody);
                    IRestResponse response = client.Execute(request);







                }





                //for (int i = 0; i < user.IDArray.Count; i++)  //foreach (var selectedUserID in selectedUsers.)
                //{
                //    User selectedUser = new User();
                //    selectedUser.ID = user.IDArray[i];
                //    selectedUser.Projects = user.Projects;
                //    selectedUser.RoleID = user.RoleID;




                //    var userFromDb = await _userRepository.GetUser(selectedUser.ID);
                //    if (userFromDb == null)
                //    {
                //        return new NotFoundResult();
                //    }
                //    selectedUser.Id = userFromDb.Id;

                //    //await _userRepository.Update(selectedUser);


                //    string baseURL = "";
                //    string authorizationValue = "Bearer " + GetAuthorizationToken();
                //    if (selectedUser.RoleID != null)
                //    {
                //Use Auth0 API to remove all users ROLES
                //baseURL = "https://wussubininja.au.auth0.com/api/v2/users/" + selectedUser.ID + "/roles";
                ////object oldRole = "{ \"roles\": [ \"" + userFromDb.RoleID + "\"] }";
                //var client = new RestClient(baseURL);


                //var request = new RestRequest(Method.DELETE);
                //request.AddHeader("content-type", "application/json");
                //request.AddHeader("authorization", authorizationValue);
                //request.AddHeader("cache-control", "no-cache");
                //request.AddParameter("application/json", oldRole, ParameterType.RequestBody);
                //IRestResponse response = client.Execute(request);

                //// Use Auth0 API to add ROLE to user
                //object newRole = "{ \"roles\": [ \"" + selectedUser.RoleID + "\"] }";
                //var request = new RestRequest(Method.POST);
                //request.AddHeader("content-type", "application/json");
                //request.AddHeader("authorization", authorizationValue);
                //request.AddHeader("cache-control", "no-cache");
                //request.AddParameter("application/json", newRole, ParameterType.RequestBody);
                //IRestResponse response = client.Execute(request);
                //    }



                //}

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
                                Issue Issue = await _issueRepository.GetIssue(issue);
                                Issue.RemoveUsers = new List<string>();
                                Issue.RemoveUsers.Add(userSelected);

                                await _issueRepository.Update(Issue);

                                await _projectManagementController.UpdateIssue(Issue);
                            }
                        }
       
                    }
                }

                UserList.Add(user);

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