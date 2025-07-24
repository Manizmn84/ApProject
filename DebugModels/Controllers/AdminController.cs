using DebugModels.Data;
using DebugModels.Models;
using DebugModels.Models.ViewModels;
using DebugModels.Services.Chat;
using DebugModels.Services.Course;
using DebugModels.Services.Department;
using DebugModels.Services.Instructor;
using DebugModels.Services.Student;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourProjectNamespace.Utilities;
using static System.Collections.Specialized.BitVector32;

namespace DebugModels.Controllers
{
    public class AdminController : Controller
    {
        private readonly ProjectContext _context;
        private readonly ICourseService _courseService;
        private readonly IDepartmentService _departmentService;
        private readonly IInstructorService _InstructorService;
        private readonly IStudentService _StudentService;
        private readonly IChatService _chatservice;
        public AdminController(ProjectContext context , ICourseService courseService, IDepartmentService departmentService,IInstructorService InstructorService,IStudentService studentService, IChatService chatService)
        {
            _context = context;
            _courseService = courseService;
            _departmentService = departmentService;
            _InstructorService = InstructorService;
            _StudentService = studentService;
            _chatservice = chatService;
        }

        public bool IsCan()
        {
            var role = HttpContext.Session.GetString("Role");
            var password = HttpContext.Session.GetString("Password");

            if (role != "Admin" || password != "rootIust402")
                return false;
            return true;
        }

        public IActionResult Index()
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            return View();
        }

        public IActionResult CreateUser()
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            return View();
        }

        [HttpPost]
        public IActionResult CreateUser(string first_name, string last_name, string email, string hashed_password)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
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
                hashed_password = PasswordHelper.HashPassword(hashed_password),
                created_at = DateTime.Now,
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("UserTable");
        }

        public IActionResult UserTable()
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var users = _context.Users.Include(users => users.UserRoles ).ThenInclude(userRole => userRole.Role).ToList();
            return View(users);

        }

        public IActionResult CreateDepartment()
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            return View();
        }

        [HttpPost]
        public IActionResult CreateDepartment(Department department)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            if (!ModelState.IsValid)
                return View(department);

            var IsExistDepartments = _context.Departments.Any(dp => dp.Name.ToLower() == department.Name.ToLower());

            if (IsExistDepartments)
            {
                ViewData["ExistDepartment"] = "We have Department with That Name";
                return View(department);
            }

            var IsRepeatBuilding = _context.Departments.Any(dp => dp.Building.ToLower() == department.Building.ToLower());

            if (IsRepeatBuilding)
            {
                ViewData["ExistDepartment"] = "In this Play have another Department";
                return View(department);
            }

            _context.Departments.Add(department);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Department created successfully.";
            return RedirectToAction("DepartmentTable");
        }

        [HttpGet]
        public IActionResult EditDepartment(int id)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var department = _context.Departments.Find(id);
            if (department == null)
            {
                ViewBag.ErrorMessage = "We dont have any Department with That ID";
                return View("Index");
            }

            return View(department);
        }

        [HttpPost]
        public IActionResult EditDepartment(Department department)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            if (!ModelState.IsValid)
                return View(department);

            var isExist = _context.Departments
                .Any(d => d.Name.ToLower() == department.Name.ToLower() && d.Id != department.Id);

            if (isExist)
            {
                ViewData["ExistDepartment"] = "A department with that name already exists.";
                return View(department);
            }

            var ExistingDepartment = _context.Departments.Find(department.Id);

            if(ExistingDepartment == null)
            {
                ViewBag.ErrorMessage = "We don`t have department with that Id";
                return View("Index");
            }

            ExistingDepartment.Name = department.Name;
            ExistingDepartment.Budget = department.Budget;
            ExistingDepartment.Building = department.Building;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Department updated successfully.";
            return RedirectToAction("DepartmentTable");
        }

        public IActionResult DepartmentTable()
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var departments = _context.Departments.ToList();
            return View(departments);
        }


        public IActionResult CreateInstructor(int userId)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = "User not found";
                return View("Index");
            }

            var departments = _context.Departments
                            .Where(dp => !dp.Instructors.Any(ins => ins.UserId == userId))
                            .ToList();

            ViewBag.Departments = departments;


            ViewBag.User = user;
            ViewBag.UserId = userId;
            return View();
        }

        [HttpPost]
        public IActionResult CreateInstructor(Instructor instructor , int userId)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = "We don't have user with this Id";
                return View("Index");
            }

            var department = _context.Departments.Include(dp => dp.Instructors).FirstOrDefault(dp => dp.Id == instructor.DepartmentId);
            if (department == null)
            {
                ViewBag.ErrorMessage = "We dont have any department with that id ";
                return View("Index");   
            }

            var existingInstructor = department.Instructors != null && department.Instructors.Any(ins => ins.UserId == userId);
            if (existingInstructor) 
            {
                ViewBag.ErrorMessage = $"We already have Instructor for the User with Id : {userId} in the Department with Id : {instructor.DepartmentId}";
                return View("Index");
            }

            var totalSalary = _context.Instructors
                                .Where(i => i.DepartmentId == instructor.DepartmentId)
                                .Sum(i => i.Salary);

            var DepartmentBudget = _context.Departments.Where(dp => dp.Id == instructor.DepartmentId).Select(dp => dp.Budget).FirstOrDefault();

            if(totalSalary + instructor.Salary > DepartmentBudget)
            {
                TempData["ErrorMessage"] = "The Instructor Salary is more than Department Budget";
                var usersData = _context.Users.Include(users => users.UserRoles).ThenInclude(userRole => userRole.Role).ToList();             
                return View("UserTable", usersData);
            }


            var Instructor = new Instructor
            {
                UserId = userId,
                Salary = instructor.Salary,
                hire_date = instructor.hire_date,
                DepartmentId = instructor.DepartmentId,

            };

            var instructorRole = _context.Roles.FirstOrDefault(u => u.name == "Instructor");
            if (instructorRole == null)
            {
                instructorRole = new Role { name = "Instructor" };

                _context.Roles.Add(instructorRole);
                _context.SaveChanges();
            }

            var existingUserRole = _context.UserRoles.FirstOrDefault(u => u.UserId == userId && u.RoleId == instructorRole.Id);
            
            if (existingUserRole == null)
            {
                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = instructorRole.Id,
                };

                _context.UserRoles.Add(userRole);
                _context.SaveChanges();
            }

            

            _context.Instructors.Add(Instructor);
            _context.SaveChanges();
            var users = _context.Users.Include(users => users.UserRoles).ThenInclude(userRole => userRole.Role).ToList();
            return RedirectToAction("UserTable",users);
        }




        public IActionResult CreateStudent(int userId)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = "User not found";
                return View("Index");
            }

            var departments = _context.Departments
                            .Where(dp => !dp.Students.Any(ins => ins.UserId == userId))
                            .ToList();

            ViewBag.Departments = departments;
            ViewBag.User = user;
            ViewBag.UserId = userId;
            return View();
        }

        [HttpPost]
        public IActionResult CreateStudent(int userId, DateTime enrollment_date, int departmentId)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = "We don't have user with this Id";
                return View("Index");
            }

            var department = _context.Departments.Include(dp => dp.Students).FirstOrDefault(dp => dp.Id == departmentId);
            if (department == null)
            {
                ViewBag.ErrorMessage = "We dont have any department with that id ";
                return View("Index");
            }

            var ExistStudentDepartment = department.Students.Any(stu => stu.UserId == userId);

            if (ExistStudentDepartment)
            {
                TempData["ErrorMessage"] = $"The Department with Id : {departmentId} have the Student with User Id : {userId}";
                var usersData = _context.Users.Include(users => users.UserRoles).ThenInclude(userRole => userRole.Role).ToList();
                return View("UserTable", usersData);
            }

                var Student = new Student
            {
                UserId = userId,
                enrollment_date = enrollment_date,
                DepartmentId = departmentId,
            };

            var studentRole = _context.Roles.FirstOrDefault(u => u.name == "Student");
            if (studentRole == null)
            {
                studentRole = new Role { name = "Student" };

                _context.Roles.Add(studentRole);
                _context.SaveChanges();
            }

            var existingUserRole = _context.UserRoles.FirstOrDefault(u => u.UserId == userId && u.RoleId == studentRole.Id);

            if (existingUserRole == null)
            {
                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = studentRole.Id,
                };

                _context.UserRoles.Add(userRole);
                _context.SaveChanges();
            }


            _context.Students.Add(Student);
            _context.SaveChanges();
            var users = _context.Users.Include(users => users.UserRoles).ThenInclude(userRole => userRole.Role).ToList();
            return RedirectToAction("UserTable", users);
        }

        public IActionResult CreateCourse()
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var departments = _context.Departments.ToList();
            ViewBag.Departments = departments;
            ViewBag.Courses = new List<Course>();
            return View();
        }

        [HttpGet]
        public JsonResult GetCoursesByDepartment(int departmentId)
        {
            var courses = _context.Courses
                .Where(c => c.DepartmentId == departmentId)
                .Select(c => new { courseId = c.CourseId, title = c.Title })
                .ToList();

            return Json(courses);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteInstructor(int instructorId)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var Result = await _InstructorService.DeleteInstructor(instructorId);

            if (!Result.Success)
            {
                TempData["ErrorMessage"] = Result.Message;
                return RedirectToAction("UserTable");
            }

            TempData["SuccessMessage"] = Result.Message;
            return RedirectToAction("UserTable");
        }


        [HttpPost]
        public IActionResult CreateCourse(Course course, int DepartmentId, List<int>? PreReqCourseIds)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            if (!int.TryParse(course.Unit, out int unitNumber))
            {
                ModelState.AddModelError("Unit", "Unit must be a valid integer number.");
            }
            else if (unitNumber <= 0)
            {
                ModelState.AddModelError("Unit", "Unit must be a positive number.");
            }
            else if (unitNumber >= 10)
            {
                ModelState.AddModelError("Unit", "Unit must be less than 10.");
            }

            var ExistCourse = _context.Courses.Any(c => c.Title.ToLower() == course.Title.ToLower() && c.DepartmentId == DepartmentId);
            if (ExistCourse)
            {
                ModelState.AddModelError("DepartmentId", "Department has this course");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Departments = _context.Departments.ToList();
                return View(course);
            }

            course.DepartmentId = DepartmentId;


            _context.Courses.Add(course);
            _context.SaveChanges();

            if(PreReqCourseIds != null && PreReqCourseIds.Count > 0)
            {
                foreach(var PreId in PreReqCourseIds)
                {
                    var PreReg = new PreRegs
                    {
                        PreRegCourseId = PreId,
                        CoureId  = course.CourseId
                    };
                    _context.PreRegs.Add(PreReg);
                }
                _context.SaveChanges();
            }
        
            TempData["SuccessMessage"] = "Course added successfully.";
            return RedirectToAction("CourseTable");
        }

        public IActionResult CourseTable()
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var courses = _context.Courses.Include(c => c.Department).Include(c => c.PreRegs).ThenInclude(pr => pr.PreRegCourse).ToList();
            return View(courses);
        }



        public IActionResult CreateSection()
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            ViewBag.Courses = _context.Courses.ToList();
            ViewBag.ClassRooms = _context.ClassRooms.ToList();
            ViewBag.AllDays = new[] { "Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday" };
            return View();
        }

        [HttpPost]
        public IActionResult CreateSection(
            int courseId,
            string building,
            int roomNumber,
            int capacity,
            string day,
            string startTime,
            string endTime,
            int semester,
            int year,
            DateTime finalExamDate,
            string description
        )
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            ViewBag.Courses = _context.Courses.ToList();
            ViewBag.ClassRooms = _context.ClassRooms.ToList();
            ViewBag.AllDays = new[] { "Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday" };

            if (string.IsNullOrWhiteSpace(description))
            {
                ViewBag.ErrorMessage = "Description is required.";
                return View();
            } 

            if (string.IsNullOrEmpty(day))
            {
                ViewBag.ErrorMessage = "Please select a day.";
                return View();
            }

            var course = _context.Courses
                .Include(c => c.PreRegs)
                .ThenInclude(p => p.PreRegCourse)
                .FirstOrDefault(c => c.CourseId == courseId);

            foreach (var prereq in course.PreRegs)
            {
                var exists = _context.Sections.Any(s => s.CourseId == prereq.PreRegCourseId);
                if (!exists)
                {
                    ViewBag.ErrorMessage = $"Prerequisite course '{prereq.PreRegCourse?.Title}' is not yet scheduled.";
                    return View();
                }
            }


            if (course == null || course.DepartmentId == null)
            {
                ViewBag.ErrorMessage = "Invalid course or department.";
                return View();
            }

            
            var department = _context.Departments.FirstOrDefault(dp => dp.Id == course.DepartmentId);

            if (!TimeSpan.TryParse(startTime, out var startTs) || !TimeSpan.TryParse(endTime, out var endTs) || startTs >= endTs)
            {
                ViewBag.ErrorMessage = "Invalid or conflicting time.";
                return View();
            }

            if (!(semester == 1 || semester == 2))
            {
                ViewBag.ErrorMessage = "Semester must be 1 or 2.";
                return View();
            }



            var examMonth = finalExamDate.Month;
            var examDay = finalExamDate.Day;
            var examYear = finalExamDate.Year;

            if (semester == 1)
            {
                bool inTerm1 = (examMonth == 9 && examDay >= 23) || 
                               (examMonth == 10) || (examMonth == 11) ||
                               (examMonth == 12) ||
                               (examMonth == 1 && examDay <= 20);   

                if (!inTerm1 || examYear != year)
                {
                    ViewBag.ErrorMessage = "For Semester 1, the final exam must be from Mehr to Dey (Sep 23 – Jan 20), and year must match the exam year.";
                    return View();
                }
            }
            else if (semester == 2)
            {
                bool isFarvardinToTir = (examMonth >= 4 && examMonth <= 7) ||
                                         (examMonth == 1 && examDay >= 21) ||
                                         (examMonth == 2) || (examMonth == 3);

                bool isBahmanToEsfand = (examMonth == 1 && examDay <= 20) || (examMonth == 12);

                if (isFarvardinToTir)
                {
                    if (examYear != year + 1)
                    {
                        ViewBag.ErrorMessage = "For Semester 2 exams in Farvardin to Tir (Jan 21 – Jul), exam year must be one more than selected year.";
                        return View();
                    }
                }
                else if (isBahmanToEsfand)
                {
                    if (examYear != year)
                    {
                        ViewBag.ErrorMessage = "For Semester 2 exams in Bahman to Esfand (Dec – Jan 20), exam year must match the selected year.";
                        return View();
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Final exam date is not valid for Semester 2.";
                    return View();
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Semester must be either 1 or 2.";
                return View();
            }



            var classRoom = _context.ClassRooms
                .FirstOrDefault(cr => cr.RoomNumber == roomNumber && cr.buliding == building);

            if (classRoom == null)
            {
                classRoom = new ClassRoom
                {
                    RoomNumber = roomNumber,
                    buliding = building,
                    Capacity = capacity
                };
                _context.ClassRooms.Add(classRoom);
                _context.SaveChanges();
            }

            var overlapping = _context.Sections
                .Include(s => s.TimeSlot)
                .Include(s => s.ClassRoom)
                .Include(s => s.Course)
                    .ThenInclude(c => c.Department)
                .Where(s =>
                    s.Course.DepartmentId == department.Id &&
                    s.ClassRoom.RoomNumber == roomNumber &&
                    s.ClassRoom.buliding == building &&
                    s.Semester == semester &&
                    s.year == year &&
                    s.TimeSlot.Day == day)
                .ToList();

            foreach (var s in overlapping)
            {
                if (s.TimeSlot == null) continue;

                var existingStart = s.TimeSlot.StartTime.TimeOfDay;
                var existingEnd = s.TimeSlot.EndTime.TimeOfDay;

                if (startTs < existingEnd && endTs > existingStart)
                {
                    ViewBag.ErrorMessage = $"Conflict on {day} in department '{department.Name}'";
                    return View();
                }
            }

            var ts = _context.TimeSlots.FirstOrDefault(t =>
                t.Day == day &&
                t.StartTime.TimeOfDay == startTs &&
                t.EndTime.TimeOfDay == endTs);

            if (ts == null)
            {
                ts = new TimeSlot
                {
                    Day = day,
                    StartTime = DateTime.Today.Add(startTs),
                    EndTime = DateTime.Today.Add(endTs)
                };
                _context.TimeSlots.Add(ts);
                _context.SaveChanges();
            }

            var titlePrefix = course.Title.Replace(" ", "").ToUpper();
            if (titlePrefix.Length > 3)
                titlePrefix = titlePrefix.Substring(0, 3);

            
            var section = new Sections
            {
                CourseId = courseId,
                ClassRoom = classRoom,
                TimeSlot = ts,
                Semester = semester,
                year = year,
                final_exam_date = finalExamDate,
                Description = description,
                Code = $"{titlePrefix}-{new Random().Next(1000, 9999)}"
            };

            _context.Sections.Add(section);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Section created successfully.";
            return View();
        }

        
        public IActionResult SelectDepartmentForSections()
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("LoginUsers", "Login");
            }

            var model = new SelectDepartmentViewModel
            {
                Departments = _context.Departments.ToList()
            };

            return View(model);
        }


        [HttpGet]
        public IActionResult SectionTable(int? SelectedDepartmentId)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("LoginUsers", "Login");
            }

            
            if (SelectedDepartmentId.HasValue)
                HttpContext.Session.SetInt32("SelectedDepartmentId", SelectedDepartmentId.Value);

            int? departmentId = SelectedDepartmentId ?? HttpContext.Session.GetInt32("SelectedDepartmentId");

            if (departmentId == null)
            {
                TempData["ErrorMessage"] = "Please select a department first.";
                return RedirectToAction("SelectDepartmentForSections");
            }

            var sections = _context.Sections
                .Include(s => s.Course).ThenInclude(c => c.Department)
                .Include(s => s.ClassRoom)
                .Include(s => s.TimeSlot)
                .Include(s => s.Teaches).ThenInclude(t => t.Instructor).ThenInclude(i => i.User)
                .Include(s => s.Takes).ThenInclude(t => t.Student).ThenInclude(st => st.User)
                .Where(s =>
                    s.Course != null &&
                    s.Course.Department != null &&
                    s.Course.DepartmentId == departmentId
                )
                .ToList();

            var grouped = sections
               .GroupBy(s => (s.year, s.Semester))
               .OrderBy(g => g.Key.year)
               .ThenBy(g => g.Key.Semester)
               .ToList();

            ViewBag.DepartmentName = _context.Departments.FirstOrDefault(d => d.Id == departmentId)?.Name;

            return View("SectionTable", grouped);
        }



        
        [HttpPost]
        public IActionResult DeleteSection(int sectionId)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var section = _context.Sections
                .Include(s => s.Teaches)
                .Include(s => s.Takes)
                .Include(s => s.TimeSlot)
                .FirstOrDefault(s => s.SectionsId == sectionId);

            if (section == null)
            {
                TempData["ErrorMessage"] = "Section not found.";
                return RedirectToAction("SectionTable");
            }

        
            if (section.Teaches != null)
            {
                _context.Teaches.Remove(section.Teaches);
            }

            
            if (section.Takes != null && section.Takes.Any())
            {
                _context.Takes.RemoveRange(section.Takes);
            }

            
            bool isTimeSlotUsedElsewhere = _context.Sections
                .Any(s => s.TimeSlotId == section.TimeSlotId && s.SectionsId != section.SectionsId);

            if (!isTimeSlotUsedElsewhere && section.TimeSlot != null)
            {
                _context.TimeSlots.Remove(section.TimeSlot);
            }


            
            _context.Sections.Remove(section);

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Section deleted successfully with all related assignments.";
            return RedirectToAction("SectionTable");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int courseId)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var Result = await _courseService.DeleteCourse(courseId);

            if (!Result.Success)
            {
                TempData["ErrorMessage"] = Result.Message;
                return RedirectToAction("CourseTable");
            }

            TempData["SuccessMessage"] = Result.Message;
            return RedirectToAction("CourseTable");
        }

        public IActionResult AssignInstructor(int sectionId)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var section = _context.Sections
                .Include(s => s.Course)
                    .ThenInclude(c => c.Department)
                .Include(s => s.ClassRoom)
                .Include(s => s.TimeSlot)
                .Include(s => s.Teaches)
                .FirstOrDefault(s => s.SectionsId == sectionId);

            if (section == null)
            {
                TempData["ErrorMessage"] = "Section not found.";
                return RedirectToAction("SectionTable");
            }

            var instructors = _context.Instructors
                .Include(i => i.User)
                .Where(i => i.DepartmentId == section.Course.DepartmentId)
                .ToList();

            ViewBag.Instructors = instructors;

            return View(section);
        }




        [HttpPost]
        public IActionResult AssignInstructor(int sectionId, int instructorId)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var section = _context.Sections
                .Include(s => s.TimeSlot)
                .Include(s => s.Teaches)
                .FirstOrDefault(s => s.SectionsId == sectionId);

            if (section == null)
            {
                TempData["ErrorMessage"] = "Section not found.";
                return RedirectToAction("SectionTable");
            }

            if (section.Teaches != null)
            {
                TempData["ErrorMessage"] = "An instructor has already been assigned to this section.";
                return RedirectToAction("SectionTable");
            }

            var instructor = _context.Instructors
                .Include(i => i.User)
                .FirstOrDefault(i => i.InstructorId == instructorId);


            if (instructor == null || instructor.UserId == null)
            {
                TempData["ErrorMessage"] = "Instructor or their user not found.";
                return RedirectToAction("SectionTable");
            }

            var userId = instructor.UserId.Value;

            
            DateTime semesterStart;

            if (section.Semester == 1)
            {
                semesterStart = new DateTime(section.year, 9, 23);
            }
            else if (section.Semester == 2)
            {
                semesterStart = new DateTime(section.year, 1, 21); 
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid semester.";
                return RedirectToAction("SectionTable");
            }

            
            if (instructor.hire_date > semesterStart)
            {
                TempData["ErrorMessage"] = "Instructor was hired after the semester started and cannot be assigned to this section.";
                return RedirectToAction("SectionTable");
            }


            bool timeConflict = _context.Sections
                .Include(s => s.TimeSlot)
                .Include(s => s.Teaches)
                    .ThenInclude(t => t.Instructor)
                .Where(s =>
                    s.Teaches != null &&
                    s.Teaches.Instructor != null &&
                    s.Teaches.Instructor.UserId == userId &&
                    s.TimeSlot != null &&
                    s.year == section.year &&             
                    s.Semester == section.Semester         
                )
                .Any(s =>
                    s.TimeSlot.Day == section.TimeSlot.Day &&
                    section.TimeSlot.StartTime.TimeOfDay < s.TimeSlot.EndTime.TimeOfDay &&
                    section.TimeSlot.EndTime.TimeOfDay > s.TimeSlot.StartTime.TimeOfDay
                );

            if (timeConflict)
            {
                TempData["ErrorMessage"] = "Instructor has a time conflict with another class in the same semester.";
                return RedirectToAction("SectionTable", new { sectionId = section.SectionsId });
            }


            bool isStudentConflict = _context.Takes
                .Include(t => t.Sections)
                    .ThenInclude(sec => sec.TimeSlot)
                .Include(t => t.Student)
                .Where(t =>
                    t.Student.UserId == userId &&
                    t.Sections.TimeSlot != null &&
                    t.Sections.year == section.year &&
                    t.Sections.Semester == section.Semester
                )
                .Any(t =>
                    t.Sections.TimeSlot.Day == section.TimeSlot.Day &&
                    section.TimeSlot.StartTime.TimeOfDay < t.Sections.TimeSlot.EndTime.TimeOfDay &&
                    section.TimeSlot.EndTime.TimeOfDay > t.Sections.TimeSlot.StartTime.TimeOfDay
                );

            if (isStudentConflict)
            {
                TempData["ErrorMessage"] = "This user is already enrolled as a student in another class at the same time this semester.";
                return RedirectToAction("SectionTable", new { sectionId = section.SectionsId });
            }



            var teaches = new Teaches
            {
                InstructorId = instructorId,
                Sections = section
            };

            _context.Teaches.Add(teaches);
            _context.SaveChanges();

            section.TeachesId = teaches.TeachesId;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Instructor assigned successfully.";
            return RedirectToAction("SectionTable");
        }

        [HttpPost]
        public async Task<IActionResult> UnassignInstructor(int sectionId)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var section = _context.Sections
                .Include(s => s.Teaches)
                .FirstOrDefault(s => s.SectionsId == sectionId);

            if (section == null)
            {
                TempData["ErrorMessage"] = "Section not found.";
                return RedirectToAction("SectionTable");
            }
            int? UserId;
            if (section.Teaches != null)
            {
                var teachesId = section.TeachesId;
                var UserInstructor = _context.Instructors.Where(ins => ins.InstructorId == section.Teaches.InstructorId).FirstOrDefault();
                UserId = UserInstructor.UserId;
                section.TeachesId = null;

                var result = await _chatservice.SendMessage(new RoleMessage
                {
                    Subject = "UnassignInstructor",
                    Content = $"Unassign from Section : {section.Code}",
                    SenderId = null,
                    ReceiverId = UserId,
                });

                var teaches = _context.Teaches.FirstOrDefault(t => t.TeachesId == teachesId);
                if (teaches != null)
                {
                    _context.Teaches.Remove(teaches);
                }

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Instructor unassigned from section successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "No instructor assigned to this section.";
            }

            return RedirectToAction("SectionTable");
        }
        public IActionResult AssignStudent(int sectionId)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var section = _context.Sections
                .Include(s => s.Course).ThenInclude(c => c.Department)
                .Include(s => s.ClassRoom)
                .Include(s => s.TimeSlot)
                .Include(s => s.Takes)
                .FirstOrDefault(s => s.SectionsId == sectionId);

            if (section == null)
            {
                TempData["ErrorMessage"] = "Section not found.";
                return RedirectToAction("SectionTable");
            }

            var deptId = section.Course?.DepartmentId;

            var students = _context.Students
                .Include(s => s.User)
                .Where(s => s.DepartmentId == deptId)
                .ToList();

            ViewBag.Section = section;
            ViewBag.Students = students;

            return View();
        }


        [HttpPost]
        public IActionResult AssignStudent(int sectionId, int studentId)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var section = _context.Sections
                .Include(s => s.Takes)
                .Include(s => s.Course).ThenInclude(c => c.Department)
                .Include(s => s.Course).ThenInclude(c => c.PreRegs)
                .Include(s => s.ClassRoom)
                .Include(s => s.TimeSlot)
                .FirstOrDefault(s => s.SectionsId == sectionId);

            if (section == null)
            {
                TempData["ErrorMessage"] = "Section not found.";
                return RedirectToAction("SectionTable");
            }

            var student = _context.Students
                .Include(s => s.User)
                .Include(s => s.Takes)
                    .ThenInclude(t => t.Sections)
                        .ThenInclude(sec => sec.Course)
                .Include(s => s.Takes)
                    .ThenInclude(t => t.Sections)
                        .ThenInclude(sec => sec.TimeSlot)
                .FirstOrDefault(s => s.StudentId == studentId);


            if (student == null || student.UserId == null)
            {
                TempData["ErrorMessage"] = "Student not found or has no associated user.";
                return RedirectToAction("AssignStudent", new { sectionId });
            }

            int userId = student.UserId.Value;


            int currentYear = section.year;
            int currentSemester = section.Semester;

            // ترم قبل (برای محاسبه GPA)
            int previousSemester = currentSemester == 1 ? 2 : 1;
            int previousYear = currentSemester == 1 ? currentYear - 1 : currentYear;

            // 🧮 گرفتن دروس ترم قبل با نمره معتبر
            var previousTermCourses = student.Takes
                .Where(t => t.Sections != null &&
                            t.Sections.year == previousYear &&
                            t.Sections.Semester == previousSemester &&
                            t.grade >= 0 &&
                            t.Sections.Course != null)
                .ToList();

            double termGPA = 0;
            int maxAllowedCredits = 20;

            if (previousTermCourses.Any())
            {
                var validCourses = previousTermCourses
                    .Where(t => int.TryParse(t.Sections.Course.Unit, out _))
                    .Select(t => new
                    {
                        Unit = int.Parse(t.Sections.Course.Unit),
                        Grade = t.grade
                    })
                    .ToList();

                int totalUnits = validCourses.Sum(x => x.Unit);
                double totalWeightedGrades = validCourses.Sum(x => x.Unit * x.Grade);

                termGPA = totalUnits > 0 ? totalWeightedGrades / totalUnits : 0;

                if (termGPA < 12)
                    maxAllowedCredits = 14;
                else if (termGPA <= 17)
                    maxAllowedCredits = 20;
                else
                    maxAllowedCredits = 24;
            }

            // ✅ فقط واحدهای همین ترم فعلی
            int totalCurrentCredits = student.Takes
                .Where(t =>
                    t.Sections?.Course != null &&
                    int.TryParse(t.Sections.Course.Unit, out _) &&
                    t.Sections.year == currentYear &&
                    t.Sections.Semester == currentSemester
                )
                .Sum(t => int.Parse(t.Sections.Course.Unit));

            // ✅ واحد درس فعلی
            int sectionCredits = 0;
            if (section.Course?.Unit != null && int.TryParse(section.Course.Unit, out int parsedUnit))
            {
                sectionCredits = parsedUnit;
            }

            // ✅ بررسی محدودیت نهایی
            if (totalCurrentCredits + sectionCredits > maxAllowedCredits)
            {
                TempData["ErrorMessage"] = $"Student's GPA in previous term: {termGPA:F2}. Max allowed: {maxAllowedCredits} credits. Currently has: {totalCurrentCredits}.";
                return RedirectToAction("AssignStudent", new { sectionId });
            }


            int currentCount = _context.Takes.Count(t => t.SectionId == sectionId);
            int maxCapacity = section.ClassRoom.Capacity;

            if (currentCount >= maxCapacity)
            {
                TempData["ErrorMessage"] = "Class is full. Cannot assign more students.";
                return RedirectToAction("AssignStudent", new { sectionId });
            }

            DateTime semesterStart;

            if (section.Semester == 1)
            {
                semesterStart = new DateTime(section.year, 9, 23); 
            }
            else if (section.Semester == 2)
            {
                semesterStart = new DateTime(section.year, 1, 21); 
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid semester.";
                return RedirectToAction("AssignStudent", new { sectionId });
            }

            
            if (student.enrollment_date > semesterStart)
            {
                TempData["ErrorMessage"] = "This student enrolled after the semester started and cannot join this class.";
                return RedirectToAction("AssignStudent", new { sectionId });
            }

            bool alreadyAssigned = _context.Takes.Any(t => t.StudentId == studentId && t.SectionId == sectionId);
            if (alreadyAssigned)
            {
                TempData["ErrorMessage"] = "This student is already assigned to this class.";
                return RedirectToAction("AssignStudent", new { sectionId });
            }

            
            

            
            if (student.DepartmentId != section.Course?.DepartmentId)
            {
                TempData["ErrorMessage"] = "This student does not belong to the same department as the course.";
                return RedirectToAction("AssignStudent", new { sectionId });
            }


            var conflict = _context.Takes
                 .Include(t => t.Sections)
                     .ThenInclude(s => s.TimeSlot)
                 .Include(t => t.Student)
                 .Where(t =>
                     t.Student.UserId == userId &&
                     t.Sections.TimeSlot != null &&
                     t.Sections.year == section.year &&           
                     t.Sections.Semester == section.Semester       
                 )
                 .Any(t =>
                     t.Sections.TimeSlot.Day == section.TimeSlot.Day &&
                     section.TimeSlot.StartTime.TimeOfDay < t.Sections.TimeSlot.EndTime.TimeOfDay &&
                     section.TimeSlot.EndTime.TimeOfDay > t.Sections.TimeSlot.StartTime.TimeOfDay
                 );

            if (conflict)
            {
                TempData["ErrorMessage"] = "Time conflict detected with another class in the same term.";
                return RedirectToAction("AssignStudent", new { sectionId });
            }



            var prereqCourseIds = section.Course?.PreRegs
                .Where(pr => pr.PreRegCourseId != null)
                .Select(pr => pr.PreRegCourseId.Value)
                .ToList();

            if (prereqCourseIds != null && prereqCourseIds.Any())
            {
                var passedOrCurrentPrereqs = student.Takes
                    .Where(t =>
                        t.Sections?.Course != null &&
                        prereqCourseIds.Contains(t.Sections.Course.CourseId) &&
                        (
                            
                            t.grade >= 10 ||

                            
                            (t.Sections.year == section.year && t.Sections.Semester == section.Semester)
                        )
                    )
                    .Select(t => t.Sections.Course.CourseId)
                    .ToList();

                var missing = prereqCourseIds.Except(passedOrCurrentPrereqs).ToList();

                if (missing.Any())
                {
                    TempData["ErrorMessage"] = "Student must have passed or be currently enrolled in prerequisite course(s).";
                    return RedirectToAction("AssignStudent", new { sectionId });
                }
            }


            bool instructorConflict = _context.Sections
                .Include(s => s.TimeSlot)
                .Include(s => s.Teaches).ThenInclude(t => t.Instructor)
                .Where(s =>
                    s.Teaches != null &&
                    s.Teaches.Instructor != null &&
                    s.Teaches.Instructor.UserId == userId &&
                    s.TimeSlot != null &&
                    s.year == section.year &&              
                    s.Semester == section.Semester         
                )
                .Any(s =>
                    s.TimeSlot.Day == section.TimeSlot.Day &&
                    section.TimeSlot.StartTime.TimeOfDay < s.TimeSlot.EndTime.TimeOfDay &&
                    section.TimeSlot.EndTime.TimeOfDay > s.TimeSlot.StartTime.TimeOfDay
                );

            if (instructorConflict)
            {
                TempData["ErrorMessage"] = "This student is also an instructor at the same time in another class during the same term.";
                return RedirectToAction("AssignStudent", new { sectionId }); 
            }



            string courseTitle = section.Course?.Title?.Trim().ToLower();

            bool duplicatePassed = student.Takes
                .Where(t => t.Sections?.Course != null &&
                            t.Sections.Course.Title.Trim().ToLower() == courseTitle &&
                            t.grade >= 10 &&
                            t.SectionId != sectionId) 
                .Any();

            if (duplicatePassed)
            {
                TempData["ErrorMessage"] = "This student has already passed a course with the same title and cannot retake it.";
                return RedirectToAction("AssignStudent", new { sectionId });
            }

            



            var take = new Takes
            {
                StudentId = studentId,
                SectionId = sectionId
            };

            _context.Takes.Add(take);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Student successfully assigned to the section.";
            return RedirectToAction("SectionTable");
        }



        [HttpPost]
        public IActionResult UnassignStudent(int takesId)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var takes = _context.Takes
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Course)
                .Include(t => t.Student)
                .FirstOrDefault(t => t.TakesId == takesId);

            if (takes == null)
            {
                TempData["ErrorMessage"] = "Student assignment not found.";
                return RedirectToAction("SectionTable");
            }

            var studentId = takes.StudentId;
            var courseId = takes.Sections?.Course?.CourseId;

            if (courseId == null)
            {
                TempData["ErrorMessage"] = "Course information is missing.";
                return RedirectToAction("SectionTable");
            }

            
            var dependentCourseIds = _context.PreRegs
                .Where(p => p.PreRegCourseId == courseId)
                .Select(p => p.CoureId)
                .ToList();

            if (dependentCourseIds.Any())
            {
                var studentCurrentCourses = _context.Takes
                    .Where(t => t.StudentId == studentId)
                    .Select(t => t.Sections.CourseId)
                    .ToList();

                var blockingCourses = dependentCourseIds.Intersect(studentCurrentCourses).ToList();
                if (blockingCourses.Any())
                {
                    TempData["ErrorMessage"] = "Cannot unassign this student. This course is a prerequisite for another course the student is currently taking.";
                    return RedirectToAction("SectionTable");
                }
            }

            
            _context.Takes.Remove(takes);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Student unassigned from section successfully.";
            return RedirectToAction("SectionTable");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var Result = await _departmentService.DeleteDepartment(id);

            if (!Result.Success)
            {
                TempData["ErrorMessage"] = Result.Message;
                return RedirectToAction("DepartmentTable");
            }

            TempData["SuccessMessage"] = Result.Message;
            return RedirectToAction("DepartmentTable");
        }

        [HttpGet]
        public async Task<IActionResult> SelectInstructorForDelete(int userId)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var user = await _context.Users
                .Include(u => u.Instructors)
                .ThenInclude(i => i.Department)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Instructors == null || user.Instructors.Count == 0)
            {
                TempData["ErrorMessage"] = "No instructor roles now found for this user.";
                return RedirectToAction("UserTable");
            }

            return View(user.Instructors);
        }

        [HttpGet]
        public async Task<IActionResult> SelectStudnetForDelete(int userId)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var user = await _context.Users
                .Include(u => u.Students)
                .ThenInclude(i => i.Department)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Students == null || user.Students.Count == 0)
            {
                TempData["ErrorMessage"] = "No Student roles now found for this user.";
                return RedirectToAction("UserTable");
            }

            return View(model:user.Students);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudent(int StudentId)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var Result = await _StudentService.DeleteStudent(StudentId);

            if (!Result.Success)
            {
                TempData["ErrorMessage"] = Result.Message;
                return RedirectToAction("UserTable");
            }

            TempData["SuccessMessage"] = Result.Message;
            return RedirectToAction("UserTable");
        }

        public IActionResult UserInfo(int userId)
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }

            var user = _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefault(u => u.Id == userId);

            var instructor = _context.Instructors
                .Include(i => i.Department)
                .FirstOrDefault(i => i.UserId == userId);

            var student = _context.Students
                .Include(s => s.Department)
                .FirstOrDefault(s => s.UserId == userId);

            var model = new UserFullInfoViewModel
            {
                User = user!,
                Instructor = instructor,
                Student = student
            };

            return View(model); 

        }


        public IActionResult Dashboard()
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            var viewModel = new AdminDashboardViewModel
            {
                DepartmentCount = _context.Departments.Count(),
                UserCount = _context.Users.Count(),
                InstructorCount = _context.Instructors.Count(),
                StudentCount = _context.Students.Count(),
                CourseCount = _context.Courses.Count(),
                SectionCount = _context.Sections.Count()
            };

            return View(viewModel);
        }

        public IActionResult Logout()
        {
            if (!IsCan())
            {
                TempData["ErrorMessage"] = "Access denied. Please login.";
                return RedirectToAction("LoginUsers", "Login");
            }
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Logout successfully.";
            return RedirectToAction("LoginUsers", "Login");
        }
    }


}
