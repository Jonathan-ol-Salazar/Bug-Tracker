using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BugTrackerDataAccess.Models;
using BugTrackerDataAccess.Repositories;
using BugTrackerDataAccess.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Bug_Tracker.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {

        private readonly IUserRepository _userRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IIssueRepository _issueRepository;


        public AccountController(IUserRepository userRepository, IProjectRepository projectRepository, IIssueRepository issueRepository)
        {
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _issueRepository = issueRepository;

        }

        public async Task Login(string returnUrl = "/")
        {

            await HttpContext.ChallengeAsync("Auth0", new AuthenticationProperties() { RedirectUri = returnUrl });


        }


        public async Task Logout()
        {
            await HttpContext.SignOutAsync("Auth0", new AuthenticationProperties
            {
                // Indicate here where Auth0 should redirect the user after a logout.
                // Note that the resulting absolute Uri must be whitelisted in the
                // **Allowed Logout URLs** settings for the app.
                RedirectUri = Url.Action("SignIn", "Home")
            });
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        //[Authorize(Roles = "Admin")]
        //[Authorize]
        public IActionResult Profile()
        {
            return View(new User()
            {
                UserName = User.Identity.Name,
                Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                //ProfileImage = User.Claims.FirstOrDefault(c => c.Type == "picture")?.Value
            });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // GETTING AUTH0 USER DETAILS OF CURRENT SIGNED IN USER
            string userID = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var currentUser = await _userRepository.GetUser(userID);
            
            if (currentUser.AccountImage == null)
            {
                var client = new RestClient("https://wussubininja.au.auth0.com/api/v2/users/" + userID);
                var request = new RestRequest(Method.GET);
                request.AddHeader("authorization", "Bearer " + GetAuthorizationToken());
                IRestResponse response = client.Execute(request);


                var response2dict = JObject.Parse(response.Content);
                currentUser.AccountImage = response2dict.SelectToken("picture").ToString();

            }


            AccountViewModel model = new AccountViewModel();

            model.User = currentUser;

            return View(model);

        }

        public string GetAuthorizationToken()
        {
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

            return Auth0ManagementAPI_AccessToken;
        }

        [HttpGet]
        public async Task<ActionResult> UpdateAccount(string ID)
        {
            //AccountViewModel model = new AccountViewModel();

            User user = await _userRepository.GetUser(ID);

            return View("UpdateAccount", user);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateAccount([Bind(include: "ID, Email, AccountImage, About, Skills, Education, Location, DOB")] User user)
        {
            if (ModelState.IsValid)
            {
                
                // CONVERT JPG TO A BYTE ARRAY
                //byte[] binaryContent = File.ReadAllBytes(user.AccountPicture);
                //byte[] binaryContent = System.IO.File.ReadAllBytes(user.AccountPicture);


                // Updating MongoDB
                var userFromDb = await _userRepository.GetUser(user.ID);
                if (userFromDb == null)
                {
                    return new NotFoundResult();
                }

                // Updating Auth0
                string data = "";
                data += "\"DOB\": \"" + user.DOB + "\",";
                data += "\"about\": \"" + user.About + "\",";
                data += "\"location\": \"" + user.Location + "\",";
                data += "\"skills\": \"" + user.Skills + "\",";
                data += "\"education\": \"" + user.Education + "\"";


                user.Id = userFromDb.Id;
                user.UserName = userFromDb.UserName;
                user.Role = userFromDb.Role;
                user.RoleID = userFromDb.RoleID;
                user.Projects = userFromDb.Projects;
                user.Issues = userFromDb.Issues;
                user.NumProjects = userFromDb.NumProjects;               
                

                await _userRepository.Update(user);

                //// Updating Auth0
                //string data; 
                //foreach (var item in )


                // Use Auth0 API to add Issue to user metadata
                string authorizationValue = "Bearer " + GetAuthorizationToken();
                string baseURL = "https://wussubininja.au.auth0.com/api/v2/users/" + user.ID;
                var client = new RestClient(baseURL);
                var request = new RestRequest(Method.PATCH);
                request.AddHeader("authorization", authorizationValue);
                request.AddHeader("content-type", "application/json");
                request.AddParameter("application/json", "{\"user_metadata\": {" + data + "}}", ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);



            }
            return RedirectToAction("Index");


        }








    }
}