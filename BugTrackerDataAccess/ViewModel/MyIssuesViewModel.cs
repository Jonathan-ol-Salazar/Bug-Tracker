using BugTrackerDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BugTrackerDataAccess.ViewModel
{
    public class MyIssuesViewModel
    {
        public IEnumerable<User> UserList { get; set; }

        public IEnumerable<Issue> IssueList { get; set; }

        public IEnumerable<Project> ProjectList { get; set; }

        public Issue Issue { get; set; }

    }
}
