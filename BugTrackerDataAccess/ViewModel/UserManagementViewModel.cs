using BugTrackerDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BugTrackerDataAccess.ViewModel
{
    public class UserManagementViewModel
    {
        public IEnumerable<User> UsersList { get; set; }

        public User User { get; set; }

        public IEnumerable<Roles> Auth0List { get; set; }

    }
}
