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
            // Sorting users assigned to selected project
            List<User> UsersAssignedList = new List<User>();
            List<User> UsersNotAssignedList = new List<User>();
            List<Issue> IssueList = new List<Issue>();

            // Model for view
            ProjectManagementViewModel model = new ProjectManagementViewModel();


            // Reset MongoDB 'Users' collection            
            await _userRepository.ResetUsers();
            Project = await _projectRepository.GetProject(Project.IDCode);


            if (Project == null)
            {
                Project = new Project();
                Project.Issues = IssueList;
            }

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
            List<User> AllUsers = new List<User>();
            List<User> ProjectManagerList = new List<User>();



            foreach (var userAuth0 in usersAuth0)
            {
                // ArrayList to store all user assigned projects
                List<string> Projects = new List<string>();
                List<string> Issues = new List<string>();


                //var Projects = new ArrayList();
                if (userAuth0.SelectToken("app_metadata").SelectToken("projects").First != null)
                {
                    // Foreach loop to add all projects for Projects ArrayList
                    foreach (var project in userAuth0.SelectToken("app_metadata").SelectToken("projects"))
                    {
                        //project.SelectToken("app_metadata").SelectToken("projects");
                        string projectString = project.ToString();
                        Projects.Add(projectString);



                    }
                }

                if (userAuth0.SelectToken("app_metadata").SelectToken("issues").First != null)
                {
                    // Foreach loop to add all projects for Projects ArrayList
                    foreach (var issue in userAuth0.SelectToken("app_metadata").SelectToken("issues"))
                    {
                        //project.SelectToken("app_metadata").SelectToken("projects");
                        string issueString = issue.ToString();
                        Issues.Add(issueString);



                    }
                }

                // GETTING ROLES ASSIGNED TO USER FROM AUTH0
                baseURL = "https://wussubininja.au.auth0.com/api/v2/users/" + userAuth0.SelectToken("user_id").ToString() + "/roles";
                client = new RestClient(baseURL);
                response = client.Execute(request);
                JArray userRolesAuth0 = JArray.Parse(response.Content);



                var user = new User();


                user.ID = userAuth0.SelectToken("user_id").ToString();
                user.UserName = userAuth0.SelectToken("name").ToString();
                user.Email = userAuth0.SelectToken("email").ToString();
                user.Role = userRolesAuth0.First.SelectToken("name").ToString();
                user.RoleID = userRolesAuth0.First.SelectToken("id").ToString();
                user.Projects = Projects;
                user.NumProjects = Projects.Count;
                user.Issues = Issues;


                AllUsers.Add(user);

                if (user.Role == "Project Manager")
                {
                    ProjectManagerList.Add(user);
                }

                //if (Project.IDCode != null)
                //{

                    //foreach (var user in AllUsers)
                    //{
                        //if (user.Projects.Contains("\"" + Project.IDCode + "\": \"" + Project.Name + "\""))
                        //{
                        //    UsersAssignedList.Add(user);

                        //}
                        //else
                        //{
                        //    UsersNotAssignedList.Add(user);
                        //}
                //}
                //}

                foreach (var project in Projects) 


                {

                    // Get IDCode from result above "IDCode" :"Name"                        
                    string projectIDCode = string.Empty;

                    if (!string.IsNullOrEmpty(project))
                    {
                        projectIDCode = project.Split(':')[0];
                        projectIDCode = projectIDCode.Replace("\"", "");
                    }

                    // Get Project
                    var projectFromDb = await _projectRepository.GetProject(projectIDCode);

                    // THIS IS TO CHECK IF PROJECTS EXISTS, REMOVE THIS AFTER YOU HAVE CLEANED THE USERS ASSIGNED PROJECTS METADATA
                    if (projectFromDb != null)
                    {
                        // this needs to be removed too
                        if (projectFromDb.AssignedUsers == null)
                        {
                            projectFromDb.AssignedUsers = new List<User>();
                        }

                        List<User> usersToBeAdded = new List<User>();
                        // Check if user is in Project 
                        foreach (var assignedUser in projectFromDb.AssignedUsers)
                        {
                            // if not, add user to list of 'Assigned Users'
                            if (assignedUser.ID != user.ID)
                            {
                                usersToBeAdded.Add(user);
                            }
                        }

                        projectFromDb.AssignedUsers = usersToBeAdded;

                        await _projectRepository.Update(projectFromDb);


                    }
                }



            }
            // Adding all the users with 'Project Manager' role to list
            Project.ProjectManagerList = ProjectManagerList;


            // Add all users from Auth0 to MongoDB 'Users' collection
            await _userRepository.AddUsers(AllUsers);


            if (Project.IDCode != null)
            {
                foreach (var user in AllUsers)
                {
                    if (user.Projects.Contains("\"" + Project.IDCode + "\": \"" + Project.Name + "\""))
                    {
                        UsersAssignedList.Add(user);

                    }
                    else
                    {
                        UsersNotAssignedList.Add(user);
                    }
                }
            }

            Project.AssignedUsers = UsersAssignedList;

            // Add list of 'Project Manager' users to 'Projects' collection
            await _projectRepository.Update(Project);


            if (Project.Issues == null)
            {
                Project.Issues = IssueList;
            }


            model.ProjectList = await _projectRepository.GetAllProjects();
            model.UsersAssignedList = UsersAssignedList;
            model.UsersNotAssignedList = UsersNotAssignedList;
            


            model.Project = Project;
            return View(model);
        }

















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
                await AddorRmove("Add", "Issue", User, selectedProject, issue, GetAuthorizationToken()); // add param to say its adding for issue

            }

            return RedirectToAction("Index", await _projectRepository.GetProject(issue.ProjectIDCode));
        }


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

                if(issue.Users == null)
                {
                    issue.Users = new List<string>();
                }

                // Adding Users 
                if (issue.AddUsers != null)
                {
                    foreach(var user in issue.AddUsers)
                    {
                        var User = await _userRepository.GetUser(user);
                        //issue.Users.Add((user + ": " + User.UserName));
                        //await _issueRepository.Update(issue);

                        await AddorRmove("Add", "Issue", User, projectFromDb, issue, GetAuthorizationToken()); // add param to say its adding for issue

                    }
                }

                // Remove Users
                if(issue.RemoveUsers != null)
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







                TempData["Message"] = "User Createed Successfully";

            }


            // UPDATING 'Projects' collection
            Issue issueOld = null;
            int indexUpdate = -1;
            foreach (var issueExisting in projectFromDb.Issues)
            {
                if (issueExisting.Id == issue.Id)
                {
                    issueOld = issueExisting;
                    indexUpdate = projectFromDb.Issues.IndexOf(issueExisting);
                    break;
                }
            }
            if (indexUpdate != -1)
            {
                projectFromDb.Issues[indexUpdate] = issue;
                await _projectRepository.Update(projectFromDb);

            }

            return RedirectToAction("Index", projectFromDb);
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


            project.ProjectManagerList = ProjectManagerList;


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

                project.ProjectManager = userFromDb;
                project.Created = DateTime.UtcNow.ToString();

                project.Updated = project.Created;                
 


                await _projectRepository.AddProject(project);


                await UpdateProjectAssignment(project);



                TempData["Message"] = "User Createed Successfully";
            }
            return RedirectToAction("Index", project);
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


        [HttpGet]
        public async Task<ActionResult> UpdateProjectDetails(string IDCode)
        {
            Project project = await _projectRepository.GetProject(IDCode);


            return View("UpdateProjectDetails", project);
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
                project.AssignedUsers = project.AssignedUsers;
                project.Issues = projectFromDb.Issues;
                //project.AddUsers = projectFromDb.AddUsers;
                //project.RemoveUsers = projectFromDb.RemoveUsers;
                project.ProjectManagerList = projectFromDb.ProjectManagerList;
                project.Created = projectFromDb.Created;
                project.Updated = DateTime.UtcNow.ToString();
                project.ProjectManager = userFromDb;

                await _projectRepository.Update(project);
                TempData["Message"] = "Customer Updated Successfully";

            }
            return RedirectToAction("Index", project);
        }


        // add param for issue or project
        public async Task<ActionResult> AddorRmove(string AddorRemove, string use, User selectedUser, Project selectedProject, Issue selectedIssue, string Auth0ManagementAPI_AccessToken)
        {
            // split into two sections - project and issue
            object data = null;
            var userFromDb = await _userRepository.GetUser(selectedUser.ID);
            
            if (userFromDb == null)
            {
                return new NotFoundResult();
            }

            if (use == "Project")
            {
                var projectFromDb = await _projectRepository.GetProject(selectedProject.IDCode);
                List<User> AssignedUsers = new List<User>();

                // If 'AssignedUsers' is empty, initialize with empty list. Else, retrive list to be updated
                if (projectFromDb.AssignedUsers == null)
                {
                    // Initialize
                    projectFromDb.AssignedUsers = AssignedUsers;
                }
                else
                {
                    // Retrive to be updated
                    AssignedUsers = projectFromDb.AssignedUsers;
                }



                string stringProject = "";
                string selectedProjectJSON = "\"" + projectFromDb.IDCode + "\": \"" + projectFromDb.Name + "\"";

                if (AddorRemove == "Add")
                {
                    AssignedUsers.Add(userFromDb);

                    stringProject = string.Join(",", userFromDb.Projects);
                    if (userFromDb.Projects.Count != 0)
                    {
                        stringProject += ",";
                    }
                    stringProject += selectedProjectJSON;
                }
                else if (AddorRemove == "Remove")
                {
                    AssignedUsers.Remove(userFromDb);


                    List<string> projectList = new List<string>();
                    foreach (var project in userFromDb.Projects)
                    {
                        if (project != selectedProjectJSON)
                        {
                            projectList.Add(project);
                        }
                        stringProject = string.Join(",", projectList);
                    }
                }

                projectFromDb.AssignedUsers = AssignedUsers;
                await _projectRepository.Update(projectFromDb);

                data = "{ \"projects\": {" + stringProject + "}}}";

            }
            else if (use == "Issue")
            {
                List<string> Users = new List<string>();
                string userIDName = selectedUser.ID + ": " + selectedUser.UserName;// + userFromDb.ToString();

                // If 'AssignedUsers' is empty, initialize with empty list. Else, retrive list to be updated
                if (selectedIssue.Users == null)
                {
                    // Initialize
                    selectedIssue.Users = Users;
                }
                else
                {
                    // Retrive to be updated
                    Users = selectedIssue.Users;
                }



                string stringIssue = "";
                string selectedIssueJSON = "\"" + selectedIssue.IDCode + "\": \"" + selectedIssue.Title + "\"";



                if (AddorRemove == "Add")
                {
                    Users.Add(userIDName);

                    stringIssue = string.Join(",", userFromDb.Issues);
                    if (userFromDb.Issues.Count != 0)
                    {
                        stringIssue += ",";
                    }
                    stringIssue += selectedIssueJSON;
                }
                else if (AddorRemove == "Remove")
                {
                    Users.Remove(userIDName);


                    List<string> issuetList = new List<string>();
                    foreach (var Issue in userFromDb.Issues)
                    {
                        if (Issue != selectedIssueJSON)
                        {

                            issuetList.Add(Issue);
                        }
                        stringIssue = string.Join(",", issuetList);
                    }
                }
                else if (AddorRemove == "Update")
                {
                    //issue
                }



                selectedIssue.Users = Users;
                await _issueRepository.Update(selectedIssue);





                data = "{ \"issues\": {" + stringIssue + "}}}";


                // If the selected project has a 'null' value for 'Issues' initialize it with a blank list
                if (selectedProject.Issues == null)
                {
                    selectedProject.Issues = new List<Issue>();
                }

                List<Issue> issueForProject = new List<Issue>();

                int counter = 0;
                bool issueExists = false;
                foreach (var issue in selectedProject.Issues)
                {
                    if (issue.IDCode == selectedIssue.IDCode)
                    {
                        issueExists = true;
                        break;
                    }
                    counter++;

                }

                issueForProject = selectedProject.Issues;


                if (issueExists == true)
                {
                    issueForProject[counter].Users = Users;
                }
                else
                {
                    issueForProject.Add(selectedIssue);
                }

                selectedProject.Issues = issueForProject;



                // Add the issue to the project and update the database
                //selectedProject.Issues = 
                await _projectRepository.Update(selectedProject);





            }

            // Use Auth0 API to add Issue to user metadata
            string authorizationValue = "Bearer " + Auth0ManagementAPI_AccessToken;
            string baseURL = "https://wussubininja.au.auth0.com/api/v2/users/" + selectedUser.ID;
            var client = new RestClient(baseURL);
            var request = new RestRequest(Method.PATCH);
            request.AddHeader("authorization", authorizationValue);
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", "{\"app_metadata\": " + data, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            //if (selectedProject != null)
            //{
            //    return await GetProjectById(selectedProject);
            //}
            //else
            //{
            //    return null; // return project
            //}

            return await GetProjectById(selectedProject);


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

                // Remove all users from project before deleting the project 
                if (project.DeleteProject == true)
                {
                    foreach (var selectedUser in project.AssignedUsers)
                    {
                        await AddorRmove("Remove", "Project", selectedUser, project, null , Auth0ManagementAPI_AccessToken);

                    }
                }

            }

            return await GetProjectById(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmDeleteProject([Bind(include: "ProjectsSelected")] ProjectManagementViewModel projectManagementViewModel)
        {
            List<Project> ProjectList = new List<Project>();


            foreach (var projectSelected in projectManagementViewModel.ProjectsSelected)
            {
                var project = await _projectRepository.GetProject(projectSelected);
                project.DeleteProject = true;
                await UpdateProjectAssignment(project);

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

            return View("DeleteConfirmation", projectManagementViewModel);
        }







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