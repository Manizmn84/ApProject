using DebugModels.Data;
using DebugModels.Models;
using DebugModels.Models.ViewModels;
using DebugModels.Utils;
using DebugModels.Services.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using YourProjectNamespace.Utilities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DebugModels.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUserService _userService;
        private readonly ProjectContext _context;

        public LoginController(IUserService userService, ProjectContext context)
        {
            _context = context;
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LoginUsers()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginUsers(LoginUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Role.ToLower() == "admin")
            {
                if (model.Email.ToLower() == "root@gmail.com" )
                {
                    if (model.Password == "rootIust402")
                    {
                        HttpContext.Session.SetString("Role", "Admin");
                        HttpContext.Session.SetString("Password", "rootIust402");
                        return RedirectToAction("UserTable","Admin");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid Admin Data";
                        return RedirectToAction("LoginUsers");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid Admin Data";
                    return RedirectToAction("LoginUsers");
                }
            }

            var Result = await _userService.LoginUser(model);

            if (!Result.Success)
            {
                TempData["ErrorMessage"] = Result.Message;
                return View(model);
            }

            var user = await _context.Users
                .Include(u => u.UserRoles!)
                    .ThenInclude(ur => ur.Role!)
                .FirstOrDefaultAsync(u =>
                    u.email.ToLower() == model.Email.ToLower() &&
                    u.hashed_password == PasswordHelper.HashPassword(model.Password) &&
                    u.UserRoles!.Any(ur => ur.Role!.name == model.Role)
                );

            TempData["UserId"] = user.Id;
            TempData["Role"] = model.Role;

            return RedirectToAction("SelectRoles");
        }

        public async Task<IActionResult> SelectRoles()
        {
            var userIdObj = TempData["UserId"];
            var role = TempData["Role"]?.ToString();

            if (userIdObj == null || role == null)
            {
                TempData["ErrorMessage"] = "Invalid user or role";
                return RedirectToAction("LoginUsers");
            }

            int userId = Convert.ToInt32(userIdObj);
            TempData.Keep();

            if (role == "Instructor")
            {
                var instructors = await _context.Instructors
                    .Include(i => i.Department)
                    .Where(i => i.UserId == userId)
                    .ToListAsync();

                return View("SelectProfile", instructors);
            }
            else if (role == "Student")
            {
                var students = await _context.Students
                    .Include(s => s.Department)
                    .Where(s => s.UserId == userId)
                    .ToListAsync();

                return View("SelectProfile", students);
            }

            TempData["ErrorMessage"] = "Unknown role";
            return RedirectToAction("LoginUsers");
        }


        [HttpPost]
        public IActionResult SelectProfileConfirmed(int UserId, string Role, int ProfileId)
        {
            HttpContext.Session.SetInt32("UserId", UserId);
            HttpContext.Session.SetString("Role", Role);
            HttpContext.Session.SetInt32("ProfileId", ProfileId);

            if (Role == "Instructor")
                return RedirectToAction("Index", "Instructor");
            else if (Role == "Student")
                return RedirectToAction("Index", "Student");

            TempData["ErrorMessage"] = "Invalid role";
            return RedirectToAction("LoginUsers");
        }



    }
}
