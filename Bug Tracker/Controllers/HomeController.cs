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


                // GETTING ROLES ASSIGNED TO USER FROM AUTH0

                // Format: https://wussubininja.au.auth0.com/api/v2/users/USER_ID/roles


                //string baseURL = "https://wussubininja.au.auth0.com/api/v2/users/" + user_ID + "/roles";
                //string authorizationValue = "Bearer " + Auth0ManagementAPI_AccessToken;
                //// Endpoint to get user role
                //client = new RestClient(baseURL);
                //request = new RestRequest(Method.GET);
                //// Add Auth0 Management API Access Token 
                //request.AddHeader("authorization", authorizationValue);
                //response = client.Execute(request);

                //// Remove the '[' and ']' from response string so it can be converted into JSON
                //response2dict = JObject.Parse(response.Content.TrimStart('[').TrimEnd(']'));
                //// Retrieving role assigned through Auth0
                //var role = response2dict.SelectToken("name").ToString();



                // Setting Role 
                string baseURL = "https://wussubininja.au.auth0.com/api/v2/users/" + user_ID + "/roles";
                string authorizationValue = "Bearer " + Auth0ManagementAPI_AccessToken;
                // Endpoint to get user role
                client = new RestClient(baseURL);
                request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("authorization", authorizationValue);
                request.AddHeader("cache-control", "no-cache");
                request.AddParameter("application/json", "{ \"roles\": [ \"ROLE_ID\" ] }", ParameterType.RequestBody);
                response = client.Execute(request);


                var x = response;


            }
            var model = await _userRepository.GetAllUsers();
            return View(model);
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
