using BugTrackerDataAccess.Models;
using System.Collections.Generic;



namespace BugTrackerDataAccess.ViewModel
{
    public class UserManagementViewModel
    {
        public IEnumerable<User> UserList { get; set; }

        public User User { get; set; }

        public IEnumerable<Role> Auth0List { get; set; }

    
        public IEnumerable<Project> ProjectList { get; set; }

    }
}
