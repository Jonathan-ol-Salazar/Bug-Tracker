﻿using BugTrackerDataAccess.Models;
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

        public IEnumerable<User> AssignedUserList { get; set; }


    }
}