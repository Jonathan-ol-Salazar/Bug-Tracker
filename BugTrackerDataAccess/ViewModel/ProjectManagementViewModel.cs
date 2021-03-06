﻿using BugTrackerDataAccess.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace BugTrackerDataAccess.ViewModel
{
    public class ProjectManagementViewModel
    {
        // MongoDB
        public IEnumerable<User> UserList { get; set; }

        // MongoDB
        public IEnumerable<Project> ProjectList { get; set; }
        
        // MongoDB
        public IEnumerable<Issue> IssueList { get; set; }

        public Project Project { get; set; }

        public Issue Issue { get; set; }

        public string ProjectIDCode { get; set; }

        public IEnumerable<User> UsersNotAssignedList { get; set; }
        public IEnumerable<User> UsersAssignedList { get; set; }

        //public IEnumerable<User> ProjectManagerList { get; set; }

        //public IEnumerable<Project> ProjectDeleteList { get; set; }
        public List<Project> ProjectDeleteList { get; set; }
        
        public List<string> ProjectsSelected { get; set; }


        public List<User> ProjectManagerList { get; set; }

        public List<string> selectedIssuesDelete { get; set; }

        public List<IFormFile> IssueImages { get; set; }

    }
}
