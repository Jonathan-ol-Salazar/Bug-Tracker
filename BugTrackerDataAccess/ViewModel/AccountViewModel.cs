using BugTrackerDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BugTrackerDataAccess.ViewModel
{
    public class AccountViewModel
    {
        public IEnumerable<Issue> IssueList { get; set; }

    }
}
