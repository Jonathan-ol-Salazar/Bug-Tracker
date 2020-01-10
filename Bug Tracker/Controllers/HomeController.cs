using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Bug_Tracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Globalization;
using RestSharp;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json.Linq;
using BugTrackerDataAccess.Repositories;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using Nancy.Json;
using MongoDB.Bson;
using BugTrackerDataAccess.Models;

namespace Bug_Tracker.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {

        private readonly IUserRepository _userRepository;

        public HomeController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }


        public async Task<IActionResult> Index()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }






}
