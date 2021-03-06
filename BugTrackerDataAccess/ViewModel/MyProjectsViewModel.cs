﻿using BugTrackerDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BugTrackerDataAccess.ViewModel
{
    public class MyProjectsViewModel
    {
        // MongoDB
        public IEnumerable<Project> ProjectList { get; set; }

        public IEnumerable<User> UserList { get; set; }

        //public IEnumerable<User> ProjectMangerList { get; set; }

        public List<User> ProjectManagerList { get; set; }


        public Project Project { get; set; }


        public IEnumerable<User> UsersNotAssignedList { get; set; }
        public IEnumerable<User> UsersAssignedList { get; set; }
        public IEnumerable<Issue> IssueList { get; set; }
    }
}
