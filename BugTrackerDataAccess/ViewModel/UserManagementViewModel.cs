﻿using BugTrackerDataAccess.Models;
using System.Collections.Generic;



namespace BugTrackerDataAccess.ViewModel
{
    public class UserManagementViewModel
    {
        // MongoDB
        public IEnumerable<User> UserList { get; set; }        

        // Auth0
        public IEnumerable<Role> RoleList { get; set; }

        // MongoDB
        public IEnumerable<Project> ProjectList { get; set; }

        public User User { get; set; }

        public List<string> selectedUsersUpdate { get; set; }

        public string selectedRole { get; set; }



        public List<string> selectedUsersDelete { get; set; }





    }
}
