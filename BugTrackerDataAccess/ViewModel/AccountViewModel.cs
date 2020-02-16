using BugTrackerDataAccess.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace BugTrackerDataAccess.ViewModel
{
    public class AccountViewModel
    {
        // List of issues
        //public IEnumerable<Issue> IssueList { get; set; }


        //public IEnumerable<Project> ProjectList { get; set; }


        public User User { get; set; }

        public List<IFormFile> AccountImages { get; set; }


    }
}
