using BugTrackerDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BugTrackerDataAccess.ViewModel
{
    public class MyProjectsViewModel
    {
        // MongoDB
        public IEnumerable<Project> ProjectList { get; set; }
    }
}
