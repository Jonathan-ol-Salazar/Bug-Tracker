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
                string issueIDCode = issue.Split(':')[0].Replace("\"", "");
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
        public async Task<ActionResult> CreateIssue([Bind(include: "IDCode, Title, Description, ProjectIDCode, Status, Submitter, AddUsers")] Issue issue)
        {
            if (ModelState.IsValid)
            {
                issue.Created = DateTime.UtcNow.ToString();
                issue.Updated = issue.Created;
                issue.Users = new List<string>();

                await _issueRepository.AddIssue(issue);


                TempData["Message"] = "User Createed Successfully";
            }

            var selectedProject = await _projectRepository.GetProject(issue.ProjectIDCode);



            foreach (var user in issue.AddUsers)
            {
                var User = await _userRepository.GetUser(user);
                await _projectManagementController.AddorRmove("Add", "Issue", User, selectedProject, issue, GetAuthorizationToken()); // add param to say its adding for issue

            }

            return RedirectToAction("Index");
        }

        //[HttpGet]
        //public async Task<ActionResult> UpdateIssue(string IDCode)
        //{
        //    ProjectManagementViewModel model = new ProjectManagementViewModel();
        //    Issue issue = await _issueRepository.GetIssue(IDCode);
        //    List<User> UsersNotAssignedList = new List<User>();
        //    List<User> UsersAssignedList = new List<User>();
        //    var allUsers = await _userRepository.GetAllUsers();
        //    List<string> AssignedUsersID = new List<string>();


        //    // Loop through all the current users assigned and add there user objects to 'UsersAssignedList' 
        //    foreach (var user in issue.Users)
        //    {
        //        string ID = user.Split(':')[0];
        //        UsersAssignedList.Add(await _userRepository.GetUser(ID));
        //        AssignedUsersID.Add(ID);
        //    }



        //    // Loop through all the user objects, if they are not in the 'UsersAssignedList', then add them to 'UsersNotAssignedList'
        //    foreach (var user in allUsers)
        //    {
        //        if (!AssignedUsersID.Contains(user.ID))
        //        {
        //            UsersNotAssignedList.Add(user);
        //        }
        //    }



        //    model.UsersAssignedList = UsersAssignedList;
        //    model.UsersNotAssignedList = UsersNotAssignedList;
        //    model.Issue = issue;
        //    return View("UpdateIssue", model);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> UpdateIssue([Bind(include: "IDCode, Title, Description, ProjectIDCode, Status, Submitter, AddUsers, RemoveUsers, Users")] Issue issue)
        //{
        //    var issueFromDb = await _issueRepository.GetIssue(issue.IDCode);
        //    var projectFromDb = await _projectRepository.GetProject(issueFromDb.ProjectIDCode);

        //    if (ModelState.IsValid)
        //    {
        //        // Updating 'Issue' collection
        //        if (projectFromDb == null || issueFromDb == null)
        //        {
        //            return new NotFoundResult();
        //        }

        //        issue.Id = issueFromDb.Id;
        //        issue.Created = issueFromDb.Created;
        //        issue.Updated = DateTime.UtcNow.ToString();

        //        if (issue.Users == null)
        //        {
        //            issue.Users = new List<string>();
        //        }

        //        // Adding Users 
        //        if (issue.AddUsers != null)
        //        {
        //            foreach (var user in issue.AddUsers)
        //            {
        //                var User = await _userRepository.GetUser(user);
        //                //issue.Users.Add((user + ": " + User.UserName));
        //                //await _issueRepository.Update(issue);

        //                await AddorRmove("Add", "Issue", User, projectFromDb, issue, GetAuthorizationToken()); // add param to say its adding for issue

        //            }
        //        }

        //        // Remove Users
        //        if (issue.RemoveUsers != null)
        //        {
        //            foreach (var user in issue.RemoveUsers)
        //            {
        //                var User = await _userRepository.GetUser(user);
        //                //issue.Users.Remove(user + ": " + User.UserName);
        //                await AddorRmove("Remove", "Issue", User, projectFromDb, issue, GetAuthorizationToken()); // add param to say its adding for issue

        //            }

        //        }

        //        await _issueRepository.Update(issue);
        //        //await AddorRmove("Update", "Issue", issue, projectFromDb, issue, GetAuthorizationToken()); // add param to say its adding for issue







        //        TempData["Message"] = "User Createed Successfully";

        //    }


        //    // UPDATING 'Projects' collection
        //    Issue issueOld = null;
        //    int indexUpdate = -1;
        //    foreach (var issueExisting in projectFromDb.Issues)
        //    {
        //        if (issueExisting.Id == issue.Id)
        //        {
        //            issueOld = issueExisting;
        //            indexUpdate = projectFromDb.Issues.IndexOf(issueExisting);
        //            break;
        //        }
        //    }
        //    if (indexUpdate != -1)
        //    {
        //        projectFromDb.Issues[indexUpdate] = issue;
        //        await _projectRepository.Update(projectFromDb);

        //    }

        //    return RedirectToAction("Index", projectFromDb);
        //}

        //public async Task<ActionResult> AddorRmove(string AddorRemove, string use, User selectedUser, Project selectedProject, Issue selectedIssue, string Auth0ManagementAPI_AccessToken)
        //{
        //    // split into two sections - project and issue
        //    object data = null;
        //    var userFromDb = await _userRepository.GetUser(selectedUser.ID);

        //    if (userFromDb == null)
        //    {
        //        return new NotFoundResult();
        //    }

        //    if (use == "Project")
        //    {
        //        var projectFromDb = await _projectRepository.GetProject(selectedProject.IDCode);
        //        List<User> AssignedUsers = new List<User>();

        //        // If 'AssignedUsers' is empty, initialize with empty list. Else, retrive list to be updated
        //        if (projectFromDb.AssignedUsers == null)
        //        {
        //            // Initialize
        //            projectFromDb.AssignedUsers = AssignedUsers;
        //        }
        //        else
        //        {
        //            // Retrive to be updated
        //            AssignedUsers = projectFromDb.AssignedUsers;
        //        }



        //        string stringProject = "";
        //        string selectedProjectJSON = "\"" + projectFromDb.IDCode + "\": \"" + projectFromDb.Name + "\"";

        //        if (AddorRemove == "Add")
        //        {
        //            AssignedUsers.Add(userFromDb);

        //            stringProject = string.Join(",", userFromDb.Projects);
        //            if (userFromDb.Projects.Count != 0)
        //            {
        //                stringProject += ",";
        //            }
        //            stringProject += selectedProjectJSON;
        //        }
        //        else if (AddorRemove == "Remove")
        //        {
        //            AssignedUsers.Remove(userFromDb);


        //            List<string> projectList = new List<string>();
        //            foreach (var project in userFromDb.Projects)
        //            {
        //                if (project != selectedProjectJSON)
        //                {
        //                    projectList.Add(project);
        //                }
        //                stringProject = string.Join(",", projectList);
        //            }
        //        }

        //        projectFromDb.AssignedUsers = AssignedUsers;
        //        await _projectRepository.Update(projectFromDb);

        //        data = "{ \"projects\": {" + stringProject + "}}}";

        //    }
        //    else if (use == "Issue")
        //    {
        //        List<string> Users = new List<string>();
        //        string userIDName = selectedUser.ID + ": " + selectedUser.UserName;// + userFromDb.ToString();

        //        // If 'AssignedUsers' is empty, initialize with empty list. Else, retrive list to be updated
        //        if (selectedIssue.Users == null)
        //        {
        //            // Initialize
        //            selectedIssue.Users = Users;
        //        }
        //        else
        //        {
        //            // Retrive to be updated
        //            Users = selectedIssue.Users;
        //        }



        //        string stringIssue = "";
        //        string selectedIssueJSON = "\"" + selectedIssue.IDCode + "\": \"" + selectedIssue.Title + "\"";



        //        if (AddorRemove == "Add")
        //        {
        //            Users.Add(userIDName);

        //            stringIssue = string.Join(",", userFromDb.Issues);
        //            if (userFromDb.Issues.Count != 0)
        //            {
        //                stringIssue += ",";
        //            }
        //            stringIssue += selectedIssueJSON;
        //        }
        //        else if (AddorRemove == "Remove")
        //        {
        //            Users.Remove(userIDName);


        //            List<string> issuetList = new List<string>();
        //            foreach (var Issue in userFromDb.Issues)
        //            {
        //                if (Issue != selectedIssueJSON)
        //                {

        //                    issuetList.Add(Issue);
        //                }
        //                stringIssue = string.Join(",", issuetList);
        //            }
        //        }
        //        else if (AddorRemove == "Update")
        //        {
        //            //issue
        //        }



        //        selectedIssue.Users = Users;
        //        await _issueRepository.Update(selectedIssue);





        //        data = "{ \"issues\": {" + stringIssue + "}}}";


        //        // If the selected project has a 'null' value for 'Issues' initialize it with a blank list
        //        if (selectedProject.Issues == null)
        //        {
        //            selectedProject.Issues = new List<Issue>();
        //        }

        //        List<Issue> issueForProject = new List<Issue>();

        //        int counter = 0;
        //        bool issueExists = false;
        //        foreach (var issue in selectedProject.Issues)
        //        {
        //            if (issue.IDCode == selectedIssue.IDCode)
        //            {
        //                issueExists = true;
        //                break;
        //            }
        //            counter++;

        //        }

        //        issueForProject = selectedProject.Issues;


        //        if (issueExists == true)
        //        {
        //            issueForProject[counter].Users = Users;
        //        }
        //        else
        //        {
        //            issueForProject.Add(selectedIssue);
        //        }

        //        selectedProject.Issues = issueForProject;



        //        // Add the issue to the project and update the database
        //        //selectedProject.Issues = 
        //        await _projectRepository.Update(selectedProject);





        //    }

        //    // Use Auth0 API to add Issue to user metadata
        //    string authorizationValue = "Bearer " + Auth0ManagementAPI_AccessToken;
        //    string baseURL = "https://wussubininja.au.auth0.com/api/v2/users/" + selectedUser.ID;
        //    var client = new RestClient(baseURL);
        //    var request = new RestRequest(Method.PATCH);
        //    request.AddHeader("authorization", authorizationValue);
        //    request.AddHeader("content-type", "application/json");
        //    request.AddParameter("application/json", "{\"app_metadata\": " + data, ParameterType.RequestBody);
        //    IRestResponse response = client.Execute(request);

        //    //if (selectedProject != null)
        //    //{
        //    //    return await GetProjectById(selectedProject);
        //    //}
        //    //else
        //    //{
        //    //    return null; // return project
        //    //}

        //    return await GetProjectById(selectedProject);


        //}

    }
}