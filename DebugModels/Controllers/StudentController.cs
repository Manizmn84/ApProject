using DebugModels.Data;
using DebugModels.Models;
using DebugModels.Models.ViewModels;
using DebugModels.Services.Chat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class StudentController : Controller
{
    private readonly ProjectContext _context;
    private readonly IChatService _ChateService;

    public StudentController(ProjectContext context, IChatService chatService)
    {
        _context = context;
        _ChateService = chatService;
    }

    public IActionResult Index()
    {
        var role = HttpContext.Session.GetString("Role");
        var profileId = HttpContext.Session.GetInt32("ProfileId");

        if (role != "Student" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var student = _context.Students
            .Include(s => s.User)
            .Include(s => s.Department)
            .Include(s => s.Takes)
                .ThenInclude(t => t.Sections)
                    .ThenInclude(sec => sec.Course)
            .FirstOrDefault(s => s.StudentId == profileId);

        if (student == null)
        {
            TempData["ErrorMessage"] = "student is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        int courseCount = student.Takes.Count;
        int passedUnits = student.Takes
            .Where(t => t.grade >= 10)
            .Sum(t => int.TryParse(t.Sections.Course?.Unit, out var u) ? u : 0);

        var viewModel = new StudentDashboardViewModel
        {
            Student = student,
            CourseCount = courseCount,
            PassedUnits = passedUnits
        };

        return View(viewModel);
    }


    public IActionResult MyCourses()
    {
        var role = HttpContext.Session.GetString("Role");
        var profileId = HttpContext.Session.GetInt32("ProfileId");

        if (role != "Student" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var student = _context.Students
            .Include(i => i.Department)
            .FirstOrDefault(s => s.StudentId == profileId);

        if (student == null)
        {
            TempData["ErrorMessage"] = "student is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var groupedCourses = _context.Takes
            .Include(t => t.Sections).ThenInclude(s => s.Course)
            .Include(t => t.Sections).ThenInclude(s => s.TimeSlot)
            .Include(t => t.Sections).ThenInclude(s => s.Teaches)
                .ThenInclude(te => te.Instructor)
                    .ThenInclude(i => i.User)
            .Where(t => t.StudentId == profileId)
            .ToList()
            .GroupBy(t => new { t.Sections.year, t.Sections.Semester }) 
            .OrderByDescending(g => g.Key.year)
            .ThenBy(g => g.Key.Semester)
            .ToList();

        return View(groupedCourses);
    }


    public IActionResult Transcript()
    {
        var role = HttpContext.Session.GetString("Role");
        var profileId = HttpContext.Session.GetInt32("ProfileId");

        if (role != "Student" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var student = _context.Students
            .Include(i => i.Department)
            .FirstOrDefault(s => s.StudentId == profileId);

        if (student == null)
        {
            TempData["ErrorMessage"] = "student is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }


        ViewBag.StudentName = $"{student.User?.first_name} {student.User?.last_name}";

   
        var transcript = _context.Takes
            .Include(t => t.Sections)
                .ThenInclude(sec => sec.Course)
            .Include(t => t.Sections)
                .ThenInclude(sec => sec.Teaches)
                    .ThenInclude(te => te.Instructor)
                        .ThenInclude(i => i.User)
            .Where(t => t.StudentId == profileId && t.grade >= 0)
            .Select(t => new
            {
                CourseTitle = t.Sections.Course.Title,
                Semester = t.Sections.Semester,
                Year = t.Sections.year,
                InstructorName = t.Sections.Teaches.Instructor.User.first_name + " " + t.Sections.Teaches.Instructor.User.last_name,
                Grade = t.grade,
                Status = t.grade >= 10 ? "Passed" : "Failed"
            }).ToList();

        return View(transcript);
    }

    public IActionResult AcademicSummary()
    {
        var role = HttpContext.Session.GetString("Role");
        var profileId = HttpContext.Session.GetInt32("ProfileId");

        if (role != "Student" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var student = _context.Students
            .Include(s => s.Takes)
                .ThenInclude(t => t.Sections)
                    .ThenInclude(sec => sec.Course)
            .FirstOrDefault(s => s.StudentId == profileId);

        if (student == null)
        {
            TempData["ErrorMessage"] = "student is Null. Please login";
            return RedirectToAction("LoginUsers", "Login");
        }



        var allCourses = student.Takes
            .Where(t => t.grade >= 0) 
            .ToList();

        var grouped = allCourses
            .GroupBy(t => new { t.Sections.year, t.Sections.Semester })
            .Select(g => new
            {
                Year = g.Key.year,
                Semester = g.Key.Semester,
                Courses = g.ToList(),
                GPA = g.Average(x => x.grade)
            })
            .OrderBy(g => g.Year)
            .ThenBy(g => g.Semester)
            .ToList();

        return View(grouped);
    }


    [HttpPost]
    public IActionResult UnassignCourse(int takesId)
    {
        var role = HttpContext.Session.GetString("Role");
        var profileId = HttpContext.Session.GetInt32("ProfileId");

        if (role != "Student" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var student = _context.Students
            .Include(i => i.Department)
            .FirstOrDefault(s => s.StudentId == profileId);

        if (student == null)
        {
            TempData["ErrorMessage"] = "student is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var take = _context.Takes.FirstOrDefault(t => t.TakesId == takesId && t.StudentId == profileId);

        if (take == null)
        {
            TempData["ErrorMessage"] = "Assignment not found.";
            return RedirectToAction("MyCourses");
        }

        if (take.grade >= 10)
        {
            TempData["ErrorMessage"] = "Cannot unassign a passed course.";
            return RedirectToAction("MyCourses");
        }

        _context.Takes.Remove(take);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Course unassigned successfully.";
        return RedirectToAction("MyCourses");
    }


    public IActionResult Logout()
    {
        HttpContext.Session.Clear(); 
        TempData["SuccessMessage"] = "Logout successfully.";
        return RedirectToAction("LoginUsers", "Login");
    }

    public async Task<IActionResult> SendMessage()
    {
        var role = HttpContext.Session.GetString("Role");
        var profileId = HttpContext.Session.GetInt32("ProfileId");

        if (role != "Student" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var student = _context.Students
            .Include(i => i.Department)
            .FirstOrDefault(s => s.StudentId == profileId);

        if (student == null)
        {
            TempData["ErrorMessage"] = "student is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var Users = await _context.Users.Where(u => u.Id != student.UserId).ToListAsync();
        ViewBag.SenderId = student.UserId;
        ViewBag.Receivers = Users;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(RoleMessage message)
    {
        var role = HttpContext.Session.GetString("Role");
        var profileId = HttpContext.Session.GetInt32("ProfileId");

        if (role != "Student" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var student = _context.Students
            .Include(i => i.Department)
            .FirstOrDefault(s => s.StudentId == profileId);

        if (student == null)
        {
            TempData["ErrorMessage"] = "student is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        if (!ModelState.IsValid)
        {
            return View(message);
        }

        var result = await _ChateService.SendMessage(message);

        TempData["SuccessMessage"] = result.Message;

        return RedirectToAction("MessageList");
    }

    public async Task<IActionResult> MessageList()
    {
        var role = HttpContext.Session.GetString("Role");
        var profileId = HttpContext.Session.GetInt32("ProfileId");

        if (role != "Student" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var student = _context.Students
            .Include(i => i.Department)
            .FirstOrDefault(s => s.StudentId == profileId);

        if (student == null)
        {
            TempData["ErrorMessage"] = "student is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var userId = student.UserId;

        var messages = await _context.RoleMessages
            .Include(rm => rm.Sender)
            .Include(rm => rm.Receiver)
            .Where(m =>
                m.Subject == "Chat" &&
                (
                    (m.SenderId == userId) ||
                    (m.ReceiverId == userId) ||
                    (m.SenderId == null && m.ReceiverId == userId) ||
                    (m.ReceiverId == null && m.SenderId == userId)
                )
            )
            .ToListAsync();



        var chatUsersEmails = messages
            .Select(m =>
                m.SenderId == userId ? m.Receiver?.email :
                m.ReceiverId == userId ? m.Sender?.email :
                m.SenderId == null ? "root@gmail.com" :
                m.ReceiverId == null ? "root@gmail.com" :
                null
            )
            .Where(email => !string.IsNullOrEmpty(email))
            .Distinct()
            .ToList();


        return View(chatUsersEmails);
    }

    public async Task<IActionResult> Chat(string withEmail)
    {

        var role = HttpContext.Session.GetString("Role");
        var profileId = HttpContext.Session.GetInt32("ProfileId");

        if (role != "Student" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var student = _context.Students
            .Include(i => i.Department)
            .FirstOrDefault(s => s.StudentId == profileId);

        if (student == null)
        {
            TempData["ErrorMessage"] = "student is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var userId = student.UserId;

        if (string.IsNullOrEmpty(withEmail))
        {
            return RedirectToAction("Index");
        }

        var user = await _context.Users.FindAsync(userId);
        var otherUser = await _context.Users.FirstOrDefaultAsync(u => u.email.ToLower() == withEmail.ToLower());
        if (user == null)
        {
            return RedirectToAction("Index");
        }
        if (otherUser == null)
        {
            return RedirectToAction("Index");
        }

        var messages = await _context.RoleMessages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m =>
                m.Subject == "Chat" &&
                (
                    (m.SenderId == userId && m.ReceiverId == otherUser.Id) ||
                    (m.SenderId == otherUser.Id && m.ReceiverId == userId) ||
                    (m.SenderId == null && m.ReceiverId == userId) ||
                    (m.SenderId == userId && m.ReceiverId == null)
                )
            )
            .OrderBy(m => m.SentAt)
            .ToListAsync();


        ViewBag.WithUserEmail = withEmail;
        ViewBag.CurrentUserEmail = user.email;

        return View(messages);
    }



    public async Task<IActionResult> Appeal()
    {
        var role = HttpContext.Session.GetString("Role");
        var profileId = HttpContext.Session.GetInt32("ProfileId");

        if (role != "Student" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var student = _context.Students
            .Include(i => i.Department)
            .FirstOrDefault(s => s.StudentId == profileId);

        if (student == null)
        {
            TempData["ErrorMessage"] = "student is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

            var sections = await _context.Takes
                    .Include(ta => ta.Sections)
                        .ThenInclude(se => se.Course)
                    .Where(ta => ta.StudentId == profileId).ToListAsync();
            ViewBag.Sections = sections;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Appeal(AppealView message)
    {
        var role = HttpContext.Session.GetString("Role");
        var profileId = HttpContext.Session.GetInt32("ProfileId");

        if (role != "Student" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var student = _context.Students
            .Include(i => i.Department)
            .FirstOrDefault(s => s.StudentId == profileId);

        if (student == null)
        {
            TempData["ErrorMessage"] = "student is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        if (!ModelState.IsValid)
            return View(message);

        var section = await _context.Sections
            .Include(s => s.Teaches)
                .ThenInclude(te => te.Instructor)
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.SectionsId == message.sectionId);

        if (section == null)
        {
            TempData["ErrorMessage"] = "don`t find Section";
            return View("Index");
        }

        int? instructorUserId = null;

        if (section?.Teaches?.Instructor?.UserId != null)
        {
            instructorUserId = section.Teaches.Instructor.UserId;
        }
        else
        {
            TempData["ErrorMessage"] = "Instructor information is not available.";
            return View("Index");
        }

        var studentUserId = student.UserId;

        if (studentUserId == null)
        {
            TempData["ErrorMessage"] = "Student User ID is not available.";
            return View("Index");
        }

        var result = await _ChateService.SendMessage(new RoleMessage
        {
            Subject = "Objection",
            Content = $"Appeal for Section-{section.Code} : " + message.Content,
            SenderId = studentUserId,
            ReceiverId = instructorUserId
        });

        TempData["SuccessMessage"] = result.Message;

        return RedirectToAction("AppealList");
    }

    public async Task<IActionResult> AppealList()
    {
        var role = HttpContext.Session.GetString("Role");
        var profileId = HttpContext.Session.GetInt32("ProfileId");

        if (role != "Student" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var student = _context.Students
            .Include(i => i.Department)
            .FirstOrDefault(s => s.StudentId == profileId);

        if (student == null)
        {
            TempData["ErrorMessage"] = "student is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var userId = student.UserId;

        var messages = await _context.RoleMessages
            .Include(rm => rm.Sender)
            .Include(rm => rm.Receiver)
            .Where(m =>
                m.Subject == "Objection" &&
                (
                    (m.SenderId == userId) ||
                    (m.ReceiverId == userId) ||
                    (m.SenderId == null && m.ReceiverId == userId) ||
                    (m.ReceiverId == null && m.SenderId == userId)
                )
            )
            .ToListAsync();



        var chatUsersEmails = messages
            .Select(m =>
                m.SenderId == userId ? m.Receiver?.email :
                m.ReceiverId == userId ? m.Sender?.email :
                m.SenderId == null ? "root@gmail.com" :
                m.ReceiverId == null ? "root@gmail.com" :
                null
            )
            .Where(email => !string.IsNullOrEmpty(email))
            .Distinct()
            .ToList();


        return View(chatUsersEmails);
    }

    public async Task<IActionResult> Objection(string withEmail)
    {
        var role = HttpContext.Session.GetString("Role");
        var profileId = HttpContext.Session.GetInt32("ProfileId");

        if (role != "Student" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var student = _context.Students
            .Include(i => i.Department)
            .FirstOrDefault(s => s.StudentId == profileId);

        if (student == null)
        {
            TempData["ErrorMessage"] = "student is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var userId = student.UserId;

        if (string.IsNullOrEmpty(withEmail))
        {
            return RedirectToAction("Index");
        }

        var user = await _context.Users.FindAsync(userId);
        var otherUser = await _context.Users.FirstOrDefaultAsync(u => u.email.ToLower() == withEmail.ToLower());
        if (user == null)
        {
            return RedirectToAction("Index");
        }
        if (otherUser == null)
        {
            return RedirectToAction("Index");
        }

        var messages = await _context.RoleMessages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m =>
                m.Subject == "Objection" &&
                (
                    (m.SenderId == userId && m.ReceiverId == otherUser.Id) ||
                    (m.SenderId == otherUser.Id && m.ReceiverId == userId) ||
                    (m.SenderId == null && m.ReceiverId == userId) ||
                    (m.SenderId == userId && m.ReceiverId == null)
                )
            )
            .OrderBy(m => m.SentAt)
            .ToListAsync();


        ViewBag.WithUserEmail = withEmail;
        ViewBag.CurrentUserEmail = user.email;

        return View(messages);
    }
}
