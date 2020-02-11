using BugTrackerDataAccess.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace BugTrackerDataAccess.ViewModel
{
    public class MyIssuesViewModel
    {
        public IEnumerable<User> UserList { get; set; }

        public IEnumerable<Issue> IssueList { get; set; }

        public List<string> ProjectList { get; set; }

        public Issue Issue { get; set; }

        public IEnumerable<User> UsersNotAssignedList { get; set; }
        public IEnumerable<User> UsersAssignedList { get; set; }

        public List<IFormFile> IssueImages { get; set; }

        public bool UserHasProjects { get; set; }

    }
}
