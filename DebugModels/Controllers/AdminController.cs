using DebugModels.Data;
using Microsoft.AspNetCore.Mvc;
using DebugModels.Models;
using Microsoft.EntityFrameworkCore;

namespace DebugModels.Controllers
{
    public class AdminController : Controller
    {
        private readonly ProjectContext _context;
        public AdminController(ProjectContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateUser(string first_name, string last_name, string email, string hashed_password)
        {
            var exsitingUser = _context.Users.FirstOrDefault(u => u.email == email);

            if (exsitingUser != null)
            {
                ViewBag.ErrorMessage = " We already have user with that emailaddress!";
                return View("Index");
            }
            var user = new User
            {
                first_name = first_name,
                last_name = last_name,
                email = email,
                hashed_password = hashed_password,
                created_at = DateTime.Now,
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("UserTable");
        }

        public IActionResult UserTable()
        {
            var users = _context.Users.Include(users => users.UserRoles ).ThenInclude(userRole => userRole.Role).ToList();
            return View(users);

        }

        public IActionResult CreateInstructor(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = "User not found";
                return View("Index");
            }

            ViewBag.User = user;
            ViewBag.UserId = userId;
            return View();
        }

        [HttpPost]
        public IActionResult CreateInstructor(int userId, decimal salary, DateTime hire_date)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = "We don't have user with this Id";
                return View("Index");
            }

            var existingInstructor = _context.Instructors.FirstOrDefault(i => i.UserId == userId);
            if (existingInstructor != null) 
            {
                ViewBag.ErrorMessage = "We already have Instructor with this ID";
                return View("Index");
            }

            var Instructor = new Instructor
            {
                UserId = userId,
                Salary = salary,
                hire_date = hire_date,

            };

            var instructorRole = _context.Roles.FirstOrDefault(u => u.name == "Instructor");
            if (instructorRole == null)
            {
                instructorRole = new Role { name = "Instructor" };

                _context.Roles.Add(instructorRole);
                _context.SaveChanges();
            }

            var existingUserRole = _context.UserRoles.FirstOrDefault(u => u.UserId == userId && u.RoleId == instructorRole.Id);

            if (existingUserRole != null)
            {
                ViewBag.ErrorMessage = "User already has Instructor";
                return View("Index");
            }

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = instructorRole.Id,
            };

            _context.UserRoles.Add(userRole);
            _context.SaveChanges();

            _context.Instructors.Add(Instructor);
            _context.SaveChanges();

            return RedirectToAction("UserTable");
        }




        public IActionResult CreateStudent(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = "User not found";
                return View("Index");
            }

            ViewBag.User = user;
            ViewBag.UserId = userId;
            return View();
        }

        [HttpPost]
        public IActionResult CreateStudent(int userId, DateTime enrollment_date)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = "We don't have user with this Id";
                return View("Index");
            }

            var existingInstructor = _context.Instructors.FirstOrDefault(i => i.UserId == userId);
            if (existingInstructor != null)
            {
                ViewBag.ErrorMessage = "We already have Instructor with this ID";
                return View("Index");
            }

            var Student = new Student
            {
                UserId = userId,
                enrollment_date = enrollment_date,
            };

            var studentRole = _context.Roles.FirstOrDefault(u => u.name == "Student");
            if (studentRole == null)
            {
                studentRole = new Role { name = "Student" };

                _context.Roles.Add(studentRole);
                _context.SaveChanges();
            }

            var existingUserRole = _context.UserRoles.FirstOrDefault(u => u.UserId == userId && u.RoleId == studentRole.Id);

            if (existingUserRole != null)
            {
                ViewBag.ErrorMessage = "User already has Student";
                return View("Index");
            }

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = studentRole.Id,
            };

            _context.UserRoles.Add(userRole);
            _context.SaveChanges();

            _context.Students.Add(Student);
            _context.SaveChanges();

            return RedirectToAction("UserTable");
        }
    }

    
}
