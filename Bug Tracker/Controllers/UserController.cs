using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BugTrackerDataAccess.Models;
using BugTrackerDataAccess.Repositories;
using Bug_Tracker.Models;


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

        [HttpGet]
        public async Task<ActionResult> ConfirmDelete(int id)
        {
            var userFromDb = await _userRepository.GetUser(id);
            return View("ConfirmDelete", userFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            var user = await _userRepository.GetUser(id);
            if (user == null)
            {
                return new NotFoundResult();
            }
            var result = await _userRepository.Delete(user.UserID);
            if (result)
            {
                TempData["Message"] = "User Deleted Successfully";
            }
            else
            {
                TempData["Message"] = "Error While Deleting the User";
            }
            return RedirectToAction("Index");
        }


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
