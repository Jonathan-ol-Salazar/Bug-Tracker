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
using System.IO;

namespace Bug_Tracker.Controllers
{
    public class MyIssuesController : Controller
    {

        private readonly IUserRepository _userRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IIssueRepository _issueRepository;
        private readonly ProjectManagementController _projectManagementController;

        public MyIssuesController(IUserRepository userRepository, IProjectRepository projectRepository, IIssueRepository issueRepository, ProjectManagementController projectManagementController)
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
        public async Task<IActionResult> Index()
        {
            // GETTING AUTH0 USER DETAILS OF CURRENT SIGNED IN USER
            string userID = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;


            List<Issue> IssueList = new List<Issue>();

            // Model for view
            MyIssuesViewModel model = new MyIssuesViewModel();


            var currentUser = await _userRepository.GetUser(userID);



            if (currentUser.Issues == null)
            {
                currentUser.Issues = new List<string>();
            }

            foreach (var issue in currentUser.Issues)
            {
                string issueIDCode = issue.Split(':')[0].Split('-')[1];
                IssueList.Add(await _issueRepository.GetIssue(issueIDCode));


            }




            if (model.IssueList == null)
            {
                model.IssueList = new List<Issue>();

            }

            model.IssueList = IssueList;
            return View(model);
        }





        [HttpGet]
        public async Task<ActionResult> CreateIssue()
        {
            // View model
            MyIssuesViewModel model = new MyIssuesViewModel();   

            // Store new Issue
            model.Issue = new Issue(); 

            // Initialize and store all users
            model.UserList = new List<User>();
            model.UserList = await _userRepository.GetAllUsers();
            // Initialize and store all projects
            model.ProjectList = new List<Project>();
            model.ProjectList = await _projectRepository.GetAllProjects();

            return View("CreateIssue", model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateIssue(MyIssuesViewModel myIssuesViewModel)
        {
            if (ModelState.IsValid)
            {
                Issue issue = myIssuesViewModel.Issue;

                issue.Created = DateTime.UtcNow.ToString();
                issue.Updated = issue.Created;
                issue.Users = new List<string>();

                foreach (var image in myIssuesViewModel.IssueImages)
                {
                    if (image.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            image.CopyTo(ms);
                            var fileBytes = ms.ToArray();
                            string fileString = Convert.ToBase64String(fileBytes);

                            // act on the Base64 data
                            issue.ScreenshotArray = fileBytes;
                            issue.ScreenshotString = fileString;
                        }
                    }
                }

                await _issueRepository.AddIssue(issue);

                var selectedProject = await _projectRepository.GetProject(issue.ProjectIDCode);

                selectedProject.Issues.Add(issue.IDCode + ": " + issue.Title);

                await _projectRepository.Update(selectedProject);

                foreach (var user in issue.AddUsers)
                {
                    var User = await _userRepository.GetUser(user);
                    await _projectManagementController.AddorRmove("Add", "Issue", User, selectedProject, issue, GetAuthorizationToken()); // add param to say its adding for issue

                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        //[ActionName("Get")]
        public async Task<ActionResult> ViewIssue(string IDCode)
        {
            var issueFromDb = await _issueRepository.GetIssue(IDCode);

            if (issueFromDb == null)
            {
                return new NotFoundResult();
            }
            return View("ViewIssue", issueFromDb);
        }

        // UPDATE

        [HttpGet]
        public async Task<ActionResult> UpdateIssue(string IDCode, string ProjectIDCode)
        {
            MyIssuesViewModel model = new MyIssuesViewModel();
            Issue issue = await _issueRepository.GetIssue(IDCode);
            List<User> UsersNotAssignedList = new List<User>();
            List<User> UsersAssignedList = new List<User>();
            var allUsers = await _userRepository.GetAllUsers();
            List<string> AssignedUsersID = new List<string>();
            //Issue issueFromDb = new Issue();

            //foreach (var issue in await _issueRepository.GetAllIssues())
            //{
            //    //if (issue.IDCode.Split(':')[0].Replace("\"", "") == issue.IDCode)
            //    //{
            //    //    issueFromDb = await _issueRepository.GetIssue(issue.IDCode);
            //    //}
            //    if (issue.ProjectIDCode == ProjectIDCode)
            //    {
            //        issueFromDb = await _issueRepository.GetIssue(issue.IDCode);

            //    }
            //}





            // Loop through all the current users assigned and add there user objects to 'UsersAssignedList' 
            foreach (var user in issue.Users)
            {
                string ID = user.Split(':')[0];
                UsersAssignedList.Add(await _userRepository.GetUser(ID));
                AssignedUsersID.Add(ID);
            }



            // Loop through all the user objects, if they are not in the 'UsersAssignedList', then add them to 'UsersNotAssignedList'
            foreach (var user in allUsers)
            {
                if (!AssignedUsersID.Contains(user.ID))
                {
                    UsersNotAssignedList.Add(user);
                }
            }



            model.UsersAssignedList = UsersAssignedList;
            model.UsersNotAssignedList = UsersNotAssignedList;
            model.Issue = issue;
            return View("UpdateIssue", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateIssue(MyIssuesViewModel myIssuesViewModel)
        {
            Issue issue = myIssuesViewModel.Issue;

            var issueFromDb = await _issueRepository.GetIssue(issue.IDCode);
            var projectFromDb = await _projectRepository.GetProject(issueFromDb.ProjectIDCode);

            //foreach(var issueInProject in projectFromDb.Issues)
            //{
            //    if(issueInProject.Split(':')[0].Replace("\"", "") == issue.IDCode)
            //    {
            //        issueFromDb = await _issueRepository.GetIssue(issue.IDCode);
            //    }
            //}

            if (ModelState.IsValid)
            {
                // Updating 'Issue' collection
                if (projectFromDb == null || issueFromDb == null)
                {
                    return new NotFoundResult();
                }

                issue.Id = issueFromDb.Id;
                issue.Created = issueFromDb.Created;
                issue.Updated = DateTime.UtcNow.ToString();

                foreach (var image in myIssuesViewModel.IssueImages)
                {
                    if (image.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            image.CopyTo(ms);
                            var fileBytes = ms.ToArray();
                            string fileString = Convert.ToBase64String(fileBytes);

                            // act on the Base64 data
                            issue.ScreenshotArray = fileBytes;
                            issue.ScreenshotString = fileString;
                        }
                    }
                }






                if (issue.Users == null)
                {
                    issue.Users = new List<string>();
                }

                //if (issueFromDb.DeleteIssue == false)
                //{
                // Adding Users 
                if (issue.AddUsers != null && issue.DeleteIssue == false)
                {
                    foreach (var user in issue.AddUsers)
                    {
                        var User = await _userRepository.GetUser(user);
                        //issue.Users.Add((user + ": " + User.UserName));
                        //await _issueRepository.Update(issue);

                        await _projectManagementController.AddorRmove("Add", "Issue", User, projectFromDb, issue, GetAuthorizationToken()); // add param to say its adding for issue

                    }
                }

                // Remove Users
                if (issue.RemoveUsers != null || issue.DeleteIssue == true)
                {
                    foreach (var user in issue.RemoveUsers)
                    {
                        var User = await _userRepository.GetUser(user);
                        //issue.Users.Remove(user + ": " + User.UserName);
                        await _projectManagementController.AddorRmove("Remove", "Issue", User, projectFromDb, issue, GetAuthorizationToken()); // add param to say its adding for issue

                    }

                }
                //}
                //else
                //{
                //    //if (issue.Users != null)
                //    //{
                //    List<string> usersDelete = issue.Users;
                //        foreach (var user in issue.Users)
                //        {
                //            var User = await _userRepository.GetUser(user);
                //            //issue.Users.Remove(user + ": " + User.UserName);
                //            await AddorRmove("Remove", "Issue", User, projectFromDb, issue, GetAuthorizationToken()); // add param to say its adding for issue

                //        }
                //    //}
                //}

                await _issueRepository.Update(issue);
                //await AddorRmove("Update", "Issue", issue, projectFromDb, issue, GetAuthorizationToken()); // add param to say its adding for issue







            }




            return RedirectToAction("Index");
        }





    }
}