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





    }
}