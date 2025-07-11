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

        public IActionResult CreateCourse()
        {
            return View();
        }

        //[HttpPost]
        //public IActionResult CreateCourse(Course course)
        //{
        //
        //    if (!ModelState.IsValid)
        //    {
        //        return View(course);
        //    }
        //
        //    var existing = _context.Courses.FirstOrDefault(c => c.Code == course.Code);
        //    if (existing != null)
        //    {
        //        ViewBag.ErrorMessage = "A course with this code already exists.";
        //        return View("Index");
        //    }
        //
        //    _context.Courses.Add(course);
        //    _context.SaveChanges();
        //
        //    TempData["SuccessMessage"] = "Course added successfully.";
        //    return RedirectToAction("CourseTable");
        //}

        public IActionResult CourseTable()
        {
            var courses = _context.Courses.ToList();
            return View(courses);
        }

        [HttpPost]
        public IActionResult DeleteCourse(int id)
        {
            var course = _context.Courses.FirstOrDefault(c => c.CourseId == id);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Course not found!";
                return RedirectToAction("CourseTable");
            }

            var relatedSections = _context.Sections
                .Where(s => s.Course != null && s.Course.CourseId == id)
                .Include(s => s.Teaches)
                .Include(s => s.TimeSlot)
                .ToList();

            foreach (var section in relatedSections)
            {
                
                if (section.Teaches != null)
                    _context.Teaches.Remove(section.Teaches);
                
                bool isTimeSlotShared = _context.Sections
                    .Any(s => s.TimeSlotId == section.TimeSlotId && s.SectionsId != section.SectionsId);

                if (!isTimeSlotShared && section.TimeSlot != null)
                    _context.TimeSlots.Remove(section.TimeSlot);

                _context.Sections.Remove(section);
            }

            _context.Courses.Remove(course);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Course and related sections were deleted successfully.";
            return RedirectToAction("CourseTable");
        }


        //[HttpPost]
        //public IActionResult EditCourse(Course course)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(course);
        //    }
        //
        //
        //    var existing = _context.Courses.FirstOrDefault(c => c.CourseId == course.CourseId);
        //    if (existing == null)
        //    {
        //        ViewBag.ErrorMessage = "Course not found!";
        //        return View("Index");
        //    }
        //
        //    existing.Title = course.Title;
        //    existing.Code = course.Code;
        //    existing.Unit = course.Unit;
        //    existing.Description = course.Description;
        //    existing.final_exam_date = course.final_exam_date;
        //
        //    _context.SaveChanges();
        //
        //    TempData["SuccessMessage"] = "Course updated successfully.";
        //    return RedirectToAction("CourseTable");
        //}

        //



        public IActionResult CreateSection()
        {
            ViewBag.Courses = _context.Courses
                .Where(c => !_context.Sections.Any(s => s.Course.CourseId == c.CourseId))
                .ToList();

            ViewBag.ClassRooms = _context.ClassRooms.ToList();
            ViewBag.AllDays = new[] { "Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday" };
            return View();
        }

        //[HttpPost]
        //public IActionResult CreateSection(
        //    int courseId,
        //    string building,
        //    int roomNumber,
        //    int capacity,
        //    [FromForm] string[] days,
        //    string startTime,
        //    string endTime,
        //    int semester,
        //    int year)
        //{
        //    ViewBag.Courses = _context.Courses
        //        .Where(c => !_context.Sections.Any(s => s.Course.CourseId == c.CourseId))
        //        .ToList();
        //    ViewBag.ClassRooms = _context.ClassRooms.ToList();
        //    ViewBag.AllDays = new[] { "Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday" };
        //
        //    if (days == null || days.Length != 1)
        //    {
        //        ViewBag.ErrorMessage = "You must select exactly one day for the section.";
        //        return View();
        //    }
        //
        //    if (semester != 1 && semester != 2)
        //    {
        //        ViewBag.ErrorMessage = "Semester must be either 1 (Fall) or 2 (Spring).";
        //        return View();
        //    }
        //
        //    var day = days[0];
        //
        //    var course = _context.Courses.FirstOrDefault(c => c.CourseId == courseId);
        //    if (course == null)
        //    {
        //        ViewBag.ErrorMessage = "Invalid course.";
        //        return View();
        //    }
        //
        //
        //    var examDate = course.final_exam_date;
        //
        //    if (semester == 1)
        //    {
        //        
        //        var fallStart = new DateTime(year, 9, 1); 
        //        var fallEnd = new DateTime(year + 1, 1, 31);
        //
        //        if (examDate < fallStart || examDate > fallEnd)
        //        {
        //            ViewBag.ErrorMessage = $"Exam date ({examDate:yyyy-MM-dd}) does not match Semester 1 (Sep {year} to Jan {year + 1}).";
        //            return View();
        //        }
        //    }
        //    else if (semester == 2)
        //    {
        //        
        //        var springStart = new DateTime(year, 2, 1); 
        //        var springEnd = new DateTime(year, 7, 31);   
        //
        //        if (examDate < springStart || examDate > springEnd)
        //        {
        //            ViewBag.ErrorMessage = $"Exam date ({examDate:yyyy-MM-dd}) does not match Semester 2 (Feb to July {year}).";
        //            return View();
        //        }
        //    }
        //    else
        //    {
        //        ViewBag.ErrorMessage = "Semester must be either 1 or 2.";
        //        return View();
        //    }
        //
        //
        //    if (!TimeSpan.TryParse(startTime, out var startTs) || !TimeSpan.TryParse(endTime, out var endTs) || startTs >= endTs)
        //    {
        //        ViewBag.ErrorMessage = "Invalid or conflicting time.";
        //        return View();
        //    }
        //
        //    var classRoom = _context.ClassRooms.FirstOrDefault(cr => cr.RoomNumber == roomNumber && cr.buliding == building);
        //    if (classRoom == null)
        //    {
        //        classRoom = new ClassRoom
        //        {
        //            RoomNumber = roomNumber,
        //            buliding = building,
        //            Capacity = capacity
        //        };
        //        _context.ClassRooms.Add(classRoom);
        //        _context.SaveChanges();
        //    }
        //
        //    var overlapping = _context.Sections
        //        .Include(s => s.TimeSlot)
        //        .Include(s => s.ClassRoom)
        //        .Where(s =>
        //            s.ClassRoom.RoomNumber == roomNumber &&
        //            s.ClassRoom.buliding == building &&
        //            s.Semester == semester &&
        //            s.year == year &&
        //            s.TimeSlot.Day == day)
        //        .ToList();
        //
        //    foreach (var s in overlapping)
        //    {
        //        if (s.TimeSlot == null) continue;
        //
        //        var existingStart = s.TimeSlot.StartTime.TimeOfDay;
        //        var existingEnd = s.TimeSlot.EndTime.TimeOfDay;
        //
        //        bool overlap = startTs < existingEnd && endTs > existingStart;
        //        if (overlap)
        //        {
        //            ViewBag.ErrorMessage = $"Conflict on {day}: Room {roomNumber} in {building} is already booked from {existingStart:hh\\:mm} to {existingEnd:hh\\:mm}.";
        //            return View();
        //        }
        //    }
        //
        //
        //    var ts = _context.TimeSlots.FirstOrDefault(t =>
        //        t.Day == day &&
        //        t.StartTime.TimeOfDay == startTs &&
        //        t.EndTime.TimeOfDay == endTs);
        //
        //    if (ts == null)
        //    {
        //        ts = new TimeSlot
        //        {
        //            Day = day,
        //            StartTime = DateTime.Today.Add(startTs),
        //            EndTime = DateTime.Today.Add(endTs)
        //        };
        //        _context.TimeSlots.Add(ts);
        //        _context.SaveChanges();
        //    }
        //
        //    var section = new Sections
        //    {
        //        Course = course,
        //        ClassRoom = classRoom,
        //        TimeSlot = ts,
        //        Semester = semester,
        //        year = year
        //    };
        //
        //    _context.Sections.Add(section);
        //    _context.SaveChanges();
        //
        //    TempData["SuccessMessage"] = "Section created successfully.";
        //    return RedirectToAction("SectionTable");
        //}


        public IActionResult SectionTable()
        {
            var sections = _context.Sections
                .Include(s => s.Course)
                .Include(s => s.ClassRoom)
                .Include(s => s.TimeSlot)
                .Include(s => s.Teaches)
                    .ThenInclude(t => t.Instructor)
                        .ThenInclude(i => i.User)
                .AsNoTracking()
                .ToList();

            return View(sections);
        }


        [HttpPost]
        public IActionResult DeleteSection(int sectionId)
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

           
            bool isTimeSlotShared = _context.Sections
                .Any(s => s.TimeSlotId == section.TimeSlotId && s.SectionsId != section.SectionsId);

            
            if (section.Teaches != null)
            {
                _context.Teaches.Remove(section.Teaches);
            }

            
            _context.Sections.Remove(section);

            
            if (!isTimeSlotShared && section.TimeSlot != null)
            {
                _context.TimeSlots.Remove(section.TimeSlot);
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Section (and related instructor, if assigned) deleted successfully.";

            return RedirectToAction("SectionTable");
        }



        ///

        public IActionResult AssignInstructor(int sectionId)
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

            if (section.TeachesId != null)
            {
                TempData["ErrorMessage"] = "This section already has an instructor assigned.";
                return RedirectToAction("SectionTable");
            }

            ViewBag.Instructors = _context.Instructors
                .Include(i => i.Teaches)
                    .ThenInclude(t => t.Sections)
                        .ThenInclude(s => s.TimeSlot)
                .Include(i => i.User)
                .ToList();

            ViewBag.Section = section;
            return View();
        }

        [HttpPost]
        public IActionResult AssignInstructor(int sectionId, int instructorId)
        {
            var section = _context.Sections
                .Include(s => s.TimeSlot)
                .FirstOrDefault(s => s.SectionsId == sectionId);

            if (section == null)
            {
                TempData["ErrorMessage"] = "Section not found.";
                return RedirectToAction("SectionTable");
            }

            if (_context.Teaches.Any(t => t.Sections.SectionsId == sectionId))
            {
                TempData["ErrorMessage"] = "This section already has an instructor.";
                return RedirectToAction("SectionTable");
            }

            var instructor = _context.Instructors
                .Include(i => i.Teaches)
                    .ThenInclude(t => t.Sections)
                        .ThenInclude(s => s.TimeSlot)
                .FirstOrDefault(i => i.InstructorId == instructorId);

            if (instructor == null)
            {
                TempData["ErrorMessage"] = "Instructor not found.";
                return RedirectToAction("SectionTable");
            }

            
            foreach (var teach in instructor.Teaches)
            {
                if (teach.Sections?.TimeSlot?.Day == section.TimeSlot?.Day)
                {
                    var existingStart = teach.Sections.TimeSlot.StartTime.TimeOfDay;
                    var existingEnd = teach.Sections.TimeSlot.EndTime.TimeOfDay;
                    var newStart = section.TimeSlot.StartTime.TimeOfDay;
                    var newEnd = section.TimeSlot.EndTime.TimeOfDay;

                    if (newStart < existingEnd && newEnd > existingStart)
                    {
                        TempData["ErrorMessage"] = "Instructor already assigned to another class at this time.";
                        return RedirectToAction("SectionTable");
                    }
                }
            }

            
            var teaches = new Teaches
            {
                Instructor = instructor,
                Sections = section
            };

            _context.Teaches.Add(teaches);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Instructor assigned successfully.";
            return RedirectToAction("SectionTable");
        }

        [HttpPost]
        public IActionResult UnassignInstructor(int sectionId)
        {
            var teaches = _context.Teaches
                .FirstOrDefault(t => t.Sections.SectionsId == sectionId);

            if (teaches == null)
            {
                TempData["ErrorMessage"] = "No instructor assigned to this section.";
                return RedirectToAction("SectionTable");
            }

            _context.Teaches.Remove(teaches);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Instructor unassigned successfully.";
            return RedirectToAction("SectionTable");
        }


        public IActionResult DeleteInstructor(int userId)
        {
            var instructor = _context.Instructors
                .FirstOrDefault(i => i.UserId == userId);

            if (instructor == null)
            {
                TempData["ErrorMessage"] = "Instructor not found.";
                return RedirectToAction("UserTable");
            }

            
            var teachesList = _context.Teaches
                .Where(t => t.InstructorId == instructor.InstructorId)
                .ToList();

            foreach (var teach in teachesList)
            {
                
                var section = _context.Sections.FirstOrDefault(s => s.TeachesId == teach.TeachesId);
                if (section != null)
                {
                    section.TeachesId = null;
                }

               
                _context.Teaches.Remove(teach);
            }

            
            var userRole = _context.UserRoles
                .FirstOrDefault(ur => ur.UserId == userId && ur.RoleId == 1); 

            if (userRole != null)
            {
                _context.UserRoles.Remove(userRole);
            }

            
            _context.Instructors.Remove(instructor);

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Instructor and related assignments removed successfully.";

            return RedirectToAction("UserTable");
        }


        public IActionResult DeleteStudent(int userId)
        {
            var student = _context.Students
                .FirstOrDefault(s => s.UserId == userId);

            if (student != null)
            {
                _context.Students.Remove(student);
            }

            var userRole = _context.UserRoles
                .FirstOrDefault(ur => ur.UserId == userId && ur.RoleId == 2); 

            if (userRole != null)
            {
                _context.UserRoles.Remove(userRole);
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Student removed successfully.";
            return RedirectToAction("UserTable");
        }



    }


}
