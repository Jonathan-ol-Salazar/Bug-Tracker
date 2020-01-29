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
using Newtonsoft.Json;
using System.Linq;

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
            // Sorting users assigned to selected project
            List<User> UsersAssignedList = new List<User>();
            List<User> UsersNotAssignedList = new List<User>();
            List<Issue> IssueList = new List<Issue>();

            // Model for view
            ProjectManagementViewModel model = new ProjectManagementViewModel();


            // Reset MongoDB 'Users' collection            
            Project = await _projectRepository.GetProject(Project.IDCode);


            if (Project == null)
            {
                Project = new Project();
                //Project.Issues = IssueList;
            }

           
            var AllUsers = await _userRepository.GetAllUsers();
            List<User> ProjectManagerList = new List<User>();


            if (Project.IDCode != null)
            {               

                if (Project.Users == null)
                {
                    Project.Users = new List<string>();
                }


                foreach (var user in Project.Users)
                {
                    UsersAssignedList.Add(await _userRepository.GetUser(user));
                }

                List<User> AllUsersList = AllUsers.ToList();
                AllUsersList.RemoveAll(x => UsersAssignedList.Any(y => y.ID == x.ID));
                UsersNotAssignedList = AllUsersList;

                foreach (var issue in Project.Issues)
                {
                    //var x = issue.Split(':')[0];
                    //x.Replace("\"", "");
                    IssueList.Add(await _issueRepository.GetIssue(issue.Split(':')[0].Replace("\"", "")));
                }         

            }

            
            model.ProjectList = await _projectRepository.GetAllProjects();
            model.UsersAssignedList = UsersAssignedList;
            model.UsersNotAssignedList = UsersNotAssignedList;
            model.IssueList = IssueList;


            model.Project = Project;
            return View(model);
        }













        /////////////////////// ISSUES ///////////////////////


        // VIEW

        [HttpGet]
        public async Task<ActionResult> ViewIssue(string IDCode)
        {
            var issueFromDb = await _issueRepository.GetIssue(IDCode);

            if (issueFromDb == null)
            {
                return new NotFoundResult();
            }
            return View("ViewIssue", issueFromDb);
        }


        // CREATE

        [HttpGet]
        public async Task<ActionResult> CreateIssue(string ProjectIDCode)
        {
            ProjectManagementViewModel model = new ProjectManagementViewModel();
            Issue issue = new Issue();

            issue.ProjectIDCode = ProjectIDCode;

            // Store Issue
            model.Issue = issue;
            // Initialize and store all users
            model.UserList = new List<User>();
            model.UserList = await _userRepository.GetAllUsers();


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

                var selectedProject = await _projectRepository.GetProject(issue.ProjectIDCode);
                
                selectedProject.Issues.Add(issue.IDCode + ": " + issue.Title);

                await _projectRepository.Update(selectedProject);

                foreach (var user in issue.AddUsers)
                {
                    var User = await _userRepository.GetUser(user);
                    await AddorRmove("Add", "Issue", User, selectedProject, issue, GetAuthorizationToken()); // add param to say its adding for issue

                }
            }

            return RedirectToAction("Index", await _projectRepository.GetProject(issue.ProjectIDCode));
        }


        // UPDATE

        [HttpGet]
        public async Task<ActionResult> UpdateIssue(string IDCode)
        {
            ProjectManagementViewModel model = new ProjectManagementViewModel();
            Issue issue = await _issueRepository.GetIssue(IDCode);
            List<User> UsersNotAssignedList = new List<User>();
            List<User> UsersAssignedList = new List<User>();
            var allUsers = await _userRepository.GetAllUsers();
            List<string> AssignedUsersID = new List<string>();


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
        public async Task<ActionResult> UpdateIssue([Bind(include: "IDCode, Title, Description, ProjectIDCode, Status, Submitter, AddUsers, RemoveUsers, Users")] Issue issue)
        {
            var issueFromDb = await _issueRepository.GetIssue(issue.IDCode);
            var projectFromDb = await _projectRepository.GetProject(issueFromDb.ProjectIDCode);

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

                if (issue.Users == null)
                {
                    issue.Users = new List<string>();
                }

                // Adding Users 
                if (issue.AddUsers != null)
                {
                    foreach (var user in issue.AddUsers)
                    {
                        var User = await _userRepository.GetUser(user);
                        //issue.Users.Add((user + ": " + User.UserName));
                        //await _issueRepository.Update(issue);

                        await AddorRmove("Add", "Issue", User, projectFromDb, issue, GetAuthorizationToken()); // add param to say its adding for issue

                    }
                }

                // Remove Users
                if (issue.RemoveUsers != null)
                {
                    foreach (var user in issue.RemoveUsers)
                    {
                        var User = await _userRepository.GetUser(user);
                        //issue.Users.Remove(user + ": " + User.UserName);
                        await AddorRmove("Remove", "Issue", User, projectFromDb, issue, GetAuthorizationToken()); // add param to say its adding for issue

                    }

                }

                await _issueRepository.Update(issue);
                //await AddorRmove("Update", "Issue", issue, projectFromDb, issue, GetAuthorizationToken()); // add param to say its adding for issue







            }




            return RedirectToAction("Index", projectFromDb);
        }



      
        
        /////////////////////// PROJECTS ///////////////////////


        public async Task<ActionResult> GetProjectById([Bind(include: "IDCode")] Project project)
        {
            var projectFromDb = await _projectRepository.GetProject(project.IDCode);
            if (projectFromDb == null)
            {
                return new NotFoundResult();
            }
            return RedirectToAction("Index", projectFromDb);
        }


        // CREATE

        [HttpGet]
        public async Task<ActionResult> CreateProject()
        {
            ProjectManagementViewModel model = new ProjectManagementViewModel();

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


                await UpdateProjectAssignment(project);



                TempData["Message"] = "User Createed Successfully";
            }
            return RedirectToAction("Index", project);
        }

        
        // UPDATE 

        [HttpGet]
        public async Task<ActionResult> UpdateProjectDetails(string IDCode)
        {
            ProjectManagementViewModel model = new ProjectManagementViewModel();

            Project Project = await _projectRepository.GetProject(IDCode);
            var AllUsers = await _userRepository.GetAllUsers();
            List<User> ProjectManagerList = new List<User>();

            foreach (var user in AllUsers)
            {
                if (user.Role == "Project Manager")
                {
                    ProjectManagerList.Add(user);
                }
            }


            model.Project = Project;
            model.ProjectManagerList = ProjectManagerList;
            return View("UpdateProjectDetails", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateProjectDetails([Bind(include: "IDCode, Name, Description, ProjectManagerUserID")] Project project)
        {
            if (ModelState.IsValid)
            {
                var projectFromDb = await _projectRepository.GetProject(project.IDCode);
                var userFromDb = await _userRepository.GetUser(project.ProjectManagerUserID);
                if (projectFromDb == null || userFromDb == null)
                {
                    return new NotFoundResult();
                }
                project.Id = projectFromDb.Id;
                //project.AssignedUsers = project.AssignedUsers;
                project.Users = projectFromDb.Users;
                project.Issues = projectFromDb.Issues;
                //project.AddUsers = projectFromDb.AddUsers;
                //project.RemoveUsers = projectFromDb.RemoveUsers;
                //project.ProjectManagerList = projectFromDb.ProjectManagerList;
                project.Created = projectFromDb.Created;
                project.Updated = DateTime.UtcNow.ToString();
                project.ProjectManagerUserID = project.ProjectManagerUserID;
                project.ProjectManagerUserName = userFromDb.UserName;

                await _projectRepository.Update(project);
                TempData["Message"] = "Customer Updated Successfully";

            }
            return RedirectToAction("Index", project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateProjectAssignment([Bind(include: "AddUsers, RemoveUsers, IDCode")] Project project)
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

                // Remove all users from project before deleting the project 
                if (project.DeleteProject != true)
                {
                    // Adding users to project
                    if (project.AddUsers != null)
                    {
                        for (int i = 0; i < project.AddUsers.Count; i++)
                        {
                            User selectedUser = new User();
                            selectedUser.ID = project.AddUsers[i];

                            await AddorRmove("Add", "Project", selectedUser, project, null, Auth0ManagementAPI_AccessToken);
                        }
                    }

                    // Removing users from project
                    if (project.RemoveUsers != null)
                    {
                        for (int i = 0; i < project.RemoveUsers.Count; i++)
                        {
                            User selectedUser = new User();
                            selectedUser.ID = project.RemoveUsers[i];

                            await AddorRmove("Remove", "Project", selectedUser, project, null, Auth0ManagementAPI_AccessToken);

                        }
                    }
                }
                else
                {
                    foreach (var selectedUser in project.Users)
                    {
                        User User = await _userRepository.GetUser(selectedUser);
                        await AddorRmove("Remove", "Project", User, project, null, Auth0ManagementAPI_AccessToken);

                    }
                }
                //// Remove all users from project before deleting the project 
                //if (project.DeleteProject == true)
                //{
                //    foreach (var selectedUser in project.Users)
                //    {
                //        User User = await _userRepository.GetUser(selectedUser);
                //        await AddorRmove("Remove", "Project", User, project, null, Auth0ManagementAPI_AccessToken);

                //    }
                //}

            }

            return await GetProjectById(project);
        }


        // DELETE

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmDeleteProject([Bind(include: "ProjectsSelected")] ProjectManagementViewModel projectManagementViewModel)
        {
            List<Project> ProjectList = new List<Project>();


            foreach (var projectSelected in projectManagementViewModel.ProjectsSelected)
            {
                var project = await _projectRepository.GetProject(projectSelected);
                project.DeleteProject = true;
                if (project.Users != null)
                {
                    await UpdateProjectAssignment(project);
                }
                ProjectList.Add(project);
            }


            var result = await _projectRepository.Delete(ProjectList);

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
        public async Task<ActionResult> DeleteProjects()
        {
            ProjectManagementViewModel model = new ProjectManagementViewModel();
            model.ProjectList = await _projectRepository.GetAllProjects();


            return View("DeleteProjects", model);
        }


        public async Task<ActionResult> DeleteProjects([Bind(include: "ProjectsSelected")] ProjectManagementViewModel projectManagementViewModel)
        {
            ProjectManagementViewModel model = new ProjectManagementViewModel();
            List<Project> ProjectList = new List<Project>();


            foreach (var projectSelected in projectManagementViewModel.ProjectsSelected)
            {
                ProjectList.Add(await _projectRepository.GetProject(projectSelected));
            }

            model = projectManagementViewModel;
            model.ProjectList = ProjectList;

            return View("DeleteConfirmation", model);
        }

        
        
        /////////////////////// ADD OR REMOVE ///////////////////////


        public async Task<ActionResult> AddorRmove(string AddorRemove, string use, User selectedUser, Project selectedProject, Issue selectedIssue, string Auth0ManagementAPI_AccessToken)
        {
            // split into two sections - project and issue
            //object data = null;
            var userFromDb = await _userRepository.GetUser(selectedUser.ID);

            if (userFromDb == null)
            {
                return new NotFoundResult();
            }

            if (use == "Project")
            {
                var projectFromDb = await _projectRepository.GetProject(selectedProject.IDCode);
                List<string> projectUsers = new List<string>();
                List<string> userProjects = new List<string>();

                // If 'Users' is empty, initialize with empty list. Else, retrive list to be updated
                if (projectFromDb.Users == null)
                {
                    // Initialize
                    projectFromDb.Users = projectUsers;
                }
                else
                {
                    // Retrive to be updated
                    projectUsers = projectFromDb.Users;
                }

                // If 'Projects' is empty, initialize with empty list. Else, retrive list to be updated
                if (userFromDb.Projects == null)
                {
                    // Initialize
                    userFromDb.Projects = userProjects;
                }
                else
                {
                    // Retrive to be updated
                    userProjects = userFromDb.Projects;
                }


                string projectIDName = projectFromDb.IDCode + ": " + projectFromDb.Name;
                // AUTH0 FORMAT
                //string projectIDName = "\"" + projectFromDb.IDCode + "\": \"" + projectFromDb.Title + "\"";

                if (AddorRemove == "Add")
                {

                    userProjects.Add(projectIDName);
                    projectUsers.Add(userFromDb.ID);

                }
                else if (AddorRemove == "Remove")
                {
                    userProjects.Remove(projectIDName);
                    projectUsers.Remove(userFromDb.ID);    
                }

                projectFromDb.Users = projectUsers;
                userFromDb.Projects = userProjects;

                await _projectRepository.Update(projectFromDb);
                await _userRepository.Update(userFromDb);
                

            }
            else if (use == "Issue")
            {
                //var issueFromDb = await _issueRepository.GetIssue(selectedIssue.IDCode);
                List<string> issueUsers = new List<string>();
                List<string> userIssues = new List<string>();
                List<string> projectIssues = new List<string>();


                // If 'Users' is empty, initialize with empty list. Else, retrive list to be updated
                if (selectedIssue.Users == null)
                {
                    // Initialize
                    selectedIssue.Users = issueUsers;
                }
                else
                {
                    // Retrive to be updated
                    issueUsers = selectedIssue.Users;
                }

                // If 'Issues' is empty, initialize with empty list. Else, retrive list to be updated
                if (userFromDb.Issues == null)
                {
                    // Initialize
                    userFromDb.Issues = userIssues;
                }
                else
                {
                    // Retrive to be updated
                    userIssues = userFromDb.Issues;
                }


                string issueIDName = selectedIssue.IDCode + ": " + selectedIssue.Title;
                // Auth0 format
                //string selectedIssueJSON = "\"" + selectedIssue.IDCode + "\": \"" + selectedIssue.Title + "\"";



                if (AddorRemove == "Add")
                {
                    issueUsers.Add(userFromDb.ID);
                    userIssues.Add(issueIDName);
                }
                else if (AddorRemove == "Remove")
                {
                    issueUsers.Remove(userFromDb.ID);
                    userIssues.Remove(issueIDName);
                }

                selectedIssue.Users = issueUsers;
                userFromDb.Issues = userIssues;

                await _issueRepository.Update(selectedIssue);
                await _userRepository.Update(userFromDb);             



      


                //// If the selected project has a 'null' value for 'Issues' initialize it with a blank list
                //if (selectedProject.Issues == null)
                //{
                //    selectedProject.Issues = new List<Issue>();
                //}

                //List<Issue> issueForProject = new List<Issue>();

                //int counter = 0;
                //bool issueExists = false;
                //foreach (var issue in selectedProject.Issues)
                //{
                //    if (issue.IDCode == selectedIssue.IDCode)
                //    {
                //        issueExists = true;
                //        break;
                //    }
                //    counter++;

                //}

                //issueForProject = selectedProject.Issues;


                //if (issueExists == true)
                //{
                //    issueForProject[counter].Users = Users;
                //}
                //else
                //{
                //    issueForProject.Add(selectedIssue);
                //}

                //selectedProject.Issues = issueForProject;



                //// Add the issue to the project and update the database
                ////selectedProject.Issues = 
                //await _projectRepository.Update(selectedProject);





            }

            //// Use Auth0 API to add Issue to user metadata
            //string authorizationValue = "Bearer " + Auth0ManagementAPI_AccessToken;
            //string baseURL = "https://wussubininja.au.auth0.com/api/v2/users/" + selectedUser.ID;
            //var client = new RestClient(baseURL);
            //var request = new RestRequest(Method.PATCH);
            //request.AddHeader("authorization", authorizationValue);
            //request.AddHeader("content-type", "application/json");
            //request.AddParameter("application/json", "{\"app_metadata\": " + data, ParameterType.RequestBody);
            //IRestResponse response = client.Execute(request);

         





            return await GetProjectById(selectedProject);


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