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

        public IActionResult CreateDepartment()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateDepartment(Department department)
        {
            if(!ModelState.IsValid)
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

        //[HttpPost]
        //public IActionResult DeleteDepartment(int id)
        //{
        //    var department = _context.Departments.Find(id);
        //    if (department == null)
        //    {
        //        TempData["ErrorMessage"] = "Department not found.";
        //        return RedirectToAction("DepartmentTable");
        //    }
        //
        //    return View(department);
        //}


        public IActionResult DepartmentTable()
        {
            var departments = _context.Departments.ToList();
            return View(departments);
        }


        public IActionResult CreateInstructor(int userId)
        {
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

        //[HttpPost]
        //public IActionResult DeleteInstructor(int instructorId)
        //{
        //    var instructor = _context.Instructors
        //        .Include(i => i.Teaches)
        //        .ThenInclude(t => t.Sections)
        //        .FirstOrDefault(i => i.InstructorId == instructorId);

        //    if (instructor == null)
        //    {
        //        TempData["ErrorMessage"] = "Instructor not found.";
        //        return RedirectToAction("UserTable");
        //    }

            
        //    var teachesList = _context.Teaches.Where(t => t.InstructorId == instructorId).ToList();

        //    foreach (var teach in teachesList)
        //    {
                
        //        var section = _context.Sections.FirstOrDefault(s => s.TeachesId == teach.TeachesId);
        //        if (section != null)
        //        {
        //            section.TeachesId = null;
        //            _context.Sections.Update(section);
        //        }

        //        _context.Teaches.Remove(teach);
        //    }

            
        //    _context.Instructors.Remove(instructor);

            
        //    var instructorRole = _context.Roles.FirstOrDefault(r => r.name == "Instructor");
        //    if (instructorRole != null)
        //    {
        //        var userRole = _context.UserRoles.FirstOrDefault(ur => ur.UserId == instructor.UserId && ur.RoleId == instructorRole.Id);
        //        if (userRole != null)
        //        {
        //            _context.UserRoles.Remove(userRole);
        //        }
        //    }

        //    _context.SaveChanges();
        //    TempData["SuccessMessage"] = "Instructor deleted successfully.";
        //    return RedirectToAction("UserTable");
        //}


        [HttpPost]
        public IActionResult CreateCourse(Course course, int DepartmentId, List<int>? PreReqCourseIds)
        {
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
            var courses = _context.Courses.Include(c => c.Department).Include(c => c.PreRegs).ThenInclude(pr => pr.PreRegCourse).ToList();
            return View(courses);
        }

         

        //[HttpPost]
        //public IActionResult DeleteCourse(int id)
        //{
        //    var course = _context.Courses
        //        .Include(c => c.Sections)
        //        .Include(c => c.PreRegs)
        //        .FirstOrDefault(c => c.CourseId == id);

        //    if (course == null)
        //    {
        //        TempData["ErrorMessage"] = "Course not found.";
        //        return RedirectToAction("CourseTable");
        //    }

        //    foreach (var sec in course.Sections.ToList())
        //    {
        //        var teaches = _context.Teaches.FirstOrDefault(t => t.TeachesId == sec.TeachesId);
        //        if (teaches != null) _context.Teaches.Remove(teaches);

        //        if (sec.TimeSlot != null)
        //        {
        //            var timeSlotId = sec.TimeSlotId;
        //            var isTimeSlotShared = _context.Sections.Any(s => s.TimeSlotId == timeSlotId && s.SectionsId != sec.SectionsId);
        //            if (!isTimeSlotShared && sec.TimeSlot != null)
        //            {
        //                _context.TimeSlots.Remove(sec.TimeSlot);
        //            }
        //        }

        //        _context.Sections.Remove(sec);
        //    }

           
        //    _context.PreRegs.RemoveRange(course.PreRegs);

            
        //    _context.Courses.Remove(course);
        //    _context.SaveChanges();

        //    TempData["SuccessMessage"] = "Course and its related sections deleted successfully.";
        //    return RedirectToAction("CourseTable");
        //}


        

        //



        public IActionResult CreateSection()
        {
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

            if (semester == 1)
            {
                bool inTerm1 = (examMonth == 9 && examDay >= 23) ||
                               (examMonth == 10) || (examMonth == 11) ||
                               (examMonth == 12) ||
                               (examMonth == 1 && examDay <= 20);

                if (!inTerm1)
                {
                    ViewBag.ErrorMessage = "Final exam date is not valid for Semester 1 (Mehr to Dey).";
                    return View();
                }
            }
            else 
            {
                bool inTerm2 = (examMonth == 1 && examDay >= 21) ||
                               (examMonth == 2) || (examMonth == 3) ||
                               (examMonth == 4) || (examMonth == 5) ||
                               (examMonth == 6) || (examMonth == 7 && examDay <= 22);

                if (!inTerm2)
                {
                    ViewBag.ErrorMessage = "Final exam date is not valid for Semester 2 (Bahman to Tir).";
                    return View();
                }
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

            int sectionCount = _context.Sections.Count() + 1;
            var section = new Sections
            {
                CourseId = courseId,
                ClassRoom = classRoom,
                TimeSlot = ts,
                Semester = semester,
                year = year,
                final_exam_date = finalExamDate,
                Description = description,
                Code = sectionCount.ToString()
            };

            _context.Sections.Add(section);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Section created successfully.";
            return RedirectToAction("SectionTable");
        }


        public IActionResult SectionTable()
        {
            var sections = _context.Sections
                .Include(s => s.Course)
                .ThenInclude(c => c.Department)
                .Include(s => s.ClassRoom)
                .Include(s => s.TimeSlot)
                .Include(s => s.Teaches)
                    .ThenInclude(t => t.Instructor)
                        .ThenInclude(i => i.User)
                .Include(s => s.Takes)
                    .ThenInclude(t => t.Student)
                        .ThenInclude(st => st.User)
                .ToList();

            return View(sections);
        }


        [HttpPost]
        public IActionResult DeleteSection(int sectionId)
        {
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

        public IActionResult AssignInstructor(int sectionId)
        {
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
                .Include(i => i.Department)
                .FirstOrDefault(i => i.InstructorId == instructorId);

            if (instructor == null)
            {
                TempData["ErrorMessage"] = "Instructor not found.";
                return RedirectToAction("SectionTable");
            }


            bool timeConflict = _context.Sections
                .Include(s => s.TimeSlot)
                .Include(s => s.Teaches)
                .Where(s => s.Teaches != null && s.Teaches.InstructorId == instructorId && s.TimeSlot != null)
                .Any(s =>
                    s.TimeSlot.Day == section.TimeSlot.Day &&
                    section.TimeSlot.StartTime.TimeOfDay < s.TimeSlot.EndTime.TimeOfDay &&
                    section.TimeSlot.EndTime.TimeOfDay > s.TimeSlot.StartTime.TimeOfDay
                );

            if (timeConflict)
            {
                TempData["ErrorMessage"] = "Instructor has a time conflict with another class.";
                return RedirectToAction("SectionTable");
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
        public IActionResult UnassignInstructor(int sectionId)
        {
            var section = _context.Sections
                .Include(s => s.Teaches)
                .FirstOrDefault(s => s.SectionsId == sectionId);

            if (section == null)
            {
                TempData["ErrorMessage"] = "Section not found.";
                return RedirectToAction("SectionTable");
            }

            if (section.Teaches != null)
            {
                var teachesId = section.TeachesId;

                
                section.TeachesId = null;

                
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
            var section = _context.Sections
                .Include(s => s.Takes)
                .Include(s => s.Course).ThenInclude(c => c.Department)
                .Include(s => s.ClassRoom)
                .Include(s => s.TimeSlot)
                .FirstOrDefault(s => s.SectionsId == sectionId);

            if (section == null)
            {
                TempData["ErrorMessage"] = "Section not found.";
                return RedirectToAction("SectionTable");
            }

            var student = _context.Students
                .Include(s => s.Takes).ThenInclude(t => t.Sections).ThenInclude(sec => sec.TimeSlot)
                .Include(s => s.Department)
                .FirstOrDefault(s => s.StudentId == studentId);

            if (student == null)
            {
                TempData["ErrorMessage"] = "Student not found.";
                return RedirectToAction("AssignStudent", new { sectionId });
            }

            
            bool alreadyAssigned = _context.Takes.Any(t => t.StudentId == studentId && t.SectionId == sectionId);
            if (alreadyAssigned)
            {
                TempData["ErrorMessage"] = "This student is already assigned to this class.";
                return RedirectToAction("AssignStudent", new { sectionId });
            }

            
            int currentCount = _context.Takes.Count(t => t.SectionId == sectionId);
            int maxCapacity = section.ClassRoom.Capacity;

            if (currentCount >= maxCapacity)
            {
                TempData["ErrorMessage"] = "Class is full. Cannot assign more students.";
                return RedirectToAction("AssignStudent", new { sectionId });
            }

            
            if (student.DepartmentId != section.Course?.DepartmentId)
            {
                TempData["ErrorMessage"] = "This student does not belong to the same department as the course.";
                return RedirectToAction("AssignStudent", new { sectionId });
            }

            
            var studentSections = student.Takes
                .Where(t => t.Sections?.TimeSlot != null)
                .Select(t => t.Sections)
                .ToList();

            foreach (var s in studentSections)
            {
                if (s.TimeSlot?.Day == section.TimeSlot?.Day)
                {
                    var start1 = s.TimeSlot.StartTime.TimeOfDay;
                    var end1 = s.TimeSlot.EndTime.TimeOfDay;
                    var start2 = section.TimeSlot.StartTime.TimeOfDay;
                    var end2 = section.TimeSlot.EndTime.TimeOfDay;

                    if (start1 < end2 && start2 < end1)
                    {
                        TempData["ErrorMessage"] = "Time conflict detected with another class.";
                        return RedirectToAction("AssignStudent", new { sectionId });
                    }
                }
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
            var takes = _context.Takes
                .Include(t => t.Sections)
                .Include(t => t.Student)
                .FirstOrDefault(t => t.TakesId == takesId);

            if (takes == null)
            {
                TempData["ErrorMessage"] = "Student assignment not found.";
                return RedirectToAction("SectionTable");
            }

            _context.Takes.Remove(takes);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Student unassigned from section successfully.";
            return RedirectToAction("SectionTable");
        }





    }


}
