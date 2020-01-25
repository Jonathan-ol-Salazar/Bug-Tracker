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

        public MyIssuesController(IUserRepository userRepository, IProjectRepository projectRepository, IIssueRepository issueRepository)
        {
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _issueRepository = issueRepository;

        }



        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // GETTING AUTH0 USER DETAILS OF CURRENT SIGNED IN USER
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;


            List<Issue> IssueList = new List<Issue>();

            // Model for view
            MyIssuesViewModel model = new MyIssuesViewModel();


            var currentUser = await _userRepository.GetUser(userId);




            foreach (var issue in currentUser.Issues)
            {
                string issueIDCode = issue.Split(':')[0].Replace("\"", "");
                //issueIDCode = issueIDCode.Replace("\"", "");

                IssueList.Add(await _issueRepository.GetIssue(issueIDCode));


            }


            if (model.IssueList == null)
            {
                model.IssueList = new List<Issue>();

            }

            model.IssueList = IssueList;
            return View(model);
        }




    }
}