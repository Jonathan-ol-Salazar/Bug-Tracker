using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BugTrackerDataAccess.Models;
using BugTrackerDataAccess.Repositories;
using Bug_Tracker.Models;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Globalization;
using RestSharp;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Bug_Tracker.Controllers
{

    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // var model = await _userRepository.GetAllUsers();

            // Reset MongoDB 'Users' collection
            await _userRepository.ResetUsers();

            if (User.Identity.IsAuthenticated)
            {
                string accessToken = await HttpContext.GetTokenAsync("access_token");

                // if you need to check the Access Token expiration time, use this value
                // provided on the authorization response and stored.
                // do not attempt to inspect/decode the access token
                DateTime accessTokenExpiresAt = DateTime.Parse(
                    await HttpContext.GetTokenAsync("expires_at"),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind);

                string idToken = await HttpContext.GetTokenAsync("id_token");

                // Now you can use them. For more info on when and how to use the
                // Access Token and ID Token, see https://auth0.com/docs/tokens

                // Reading JWT idToken
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(idToken);
                string user_ID = token.Payload.Sub;


                // ACCESS TOKEN FOR AUTH0 MANAGEMENT API

                var client = new RestClient("https://wussubininja.au.auth0.com/oauth/token");
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddParameter("application/x-www-form-urlencoded", "grant_type=client_credentials&client_id=LZ1ZnJCpRTSZB4b2iET97KhOajNiPyLk&client_secret=6Actr7Xa1tNRC6370iM6rzD68Wbpq8UCurK3QbtBiRRAUZqheOwFzDspQkZ2-7QJ&audience=https://wussubininja.au.auth0.com/api/v2/", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);

                // Parsing into JSON 
                var response2dict = JObject.Parse(response.Content);
                // Retrieving Access Token
                var Auth0ManagementAPI_AccessToken = response2dict.First.First.ToString();




                // GETTING ALL USERS
                string baseURL = "https://wussubininja.au.auth0.com/api/v2/users";
                string authorizationValue = "Bearer " + Auth0ManagementAPI_AccessToken;
                // Endpoint to get user role
                client = new RestClient(baseURL);
                request = new RestRequest(Method.GET);
                // Add Auth0 Management API Access Token 
                request.AddHeader("authorization", authorizationValue);
                response = client.Execute(request);

                var content = response.Content;
                JArray contentArray = JArray.Parse(content);
                var Users = new List<User>();

                foreach (var userAuth0 in contentArray)
                {
                    var document = new User();

                    document.ID = userAuth0.SelectToken("user_id").ToString();
                    document.UserName = userAuth0.SelectToken("name").ToString();
                    document.Email = userAuth0.SelectToken("email").ToString();
                    //document.Id = userAuth0.SelectToken("email").ToString();
                    //  document.Role = userAuth0.SelectToken("app_metadata").SelectToken("roles").ToString();

                    Users.Add(document);
                }

                await _userRepository.AddUsers(Users);

            


                //// GETTING ROLES ASSIGNED TO USER FROM AUTH0
                //// Format: https://wussubininja.au.auth0.com/api/v2/users/USER_ID/roles

                //string baseURL = "https://wussubininja.au.auth0.com/api/v2/users/" + user_ID + "/roles";
                //string authorizationValue = "Bearer " + Auth0ManagementAPI_AccessToken;
                //// Endpoint to get user role
                //client = new RestClient(baseURL);
                //request = new RestRequest(Method.GET);
                //// Add Auth0 Management API Access Token 
                //request.AddHeader("authorization", authorizationValue);
                //response = client.Execute(request);


                //// If no role is set, 'Submitter' role will be selected
                //if (response.ContentLength == -1)
                //{
                //    request = new RestRequest(Method.POST);
                //    request.AddHeader("content-type", "application/json");
                //    request.AddHeader("authorization", authorizationValue);
                //    request.AddHeader("cache-control", "no-cache");
                //    request.AddParameter("application/json", "{ \"roles\": [ \"rol_fWiLOHdB4uUAg3Fq\" ] }", ParameterType.RequestBody);
                //    response = client.Execute(request);
                //}


            }

            var model = await _userRepository.GetAllUsers();

            return View(model);
        }

        [HttpGet]
        //[ActionName("Get")]
        public async Task<ActionResult> GetUserById(int id)
        {
            var user = await _userRepository.GetUser(id);
            if (user == null)
            {
                return new NotFoundResult();
            }
            return View("GetUserById", user);
        }


        [HttpGet]
        public ActionResult Create()
        {
            return View("Create", new User());
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(include: "UserID, UserName, Email, Role")] User user)
        {
            if (ModelState.IsValid)
            {
                await _userRepository.Create(user);
                TempData["Message"] = "User Createed Successfully";

            }
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<ActionResult> Update(int id)
        {
            User user = await _userRepository.GetUser(id);
            return View("Update", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update([Bind(include: "UserID, UserName, Email, Role")] User user)
        {
            if (ModelState.IsValid)
            {
                var userFromDb = await _userRepository.GetUser(user.UserID);
                if (userFromDb == null)
                {
                    return new NotFoundResult();
                }
                user.Id = userFromDb.Id;
                await _userRepository.Update(user);
                TempData["Message"] = "Customer Updated Successfully";

            }
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> ConfirmDelete(int id)
        {
            var userFromDb = await _userRepository.GetUser(id);
            return View("ConfirmDelete", userFromDb);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Delete(int id)
        //{
        //    var user = await _userRepository.GetUser(id);
        //    if (user == null)
        //    {
        //        Console.WriteLine("OMFL");
        //        return new NotFoundResult();

        //    }
        //    var result = await _userRepository.Delete(user.ID);
        //    if (result)
        //    {
        //        TempData["Message"] = "User Deleted Successfully";
        //    }
        //    else
        //    {
        //        TempData["Message"] = "Error While Deleting the User";
        //    }
        //    return RedirectToAction("Index");
        //}


        //public IActionResult Index()
        //{
        //    return View();
        //}

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