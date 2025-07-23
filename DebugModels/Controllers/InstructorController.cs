using DebugModels.Data;
using DebugModels.Models;
using DebugModels.Models.ViewModels;
using DebugModels.Services.Chat;
using DebugModels.Services.Instructor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

public class InstructorController : Controller
{
    private readonly ProjectContext _context;
    private readonly IInstructorService _InstructorService;
    private readonly IChatService _ChateService;

    public InstructorController(ProjectContext context , IInstructorService InstructorService, IChatService ChatService)
    {
        _context = context;
        _InstructorService = InstructorService;
        _ChateService = ChatService;
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();

        TempData["SuccessMessage"] = "Logout Successfully";


        return RedirectToAction("LoginUsers", "Login");
    }


    public async Task<IActionResult> Index()
    {
        var role = HttpContext.Session.GetString("Role");
        var profileId = HttpContext.Session.GetInt32("ProfileId");

        if (role != "Instructor" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var instructor = await _context.Instructors
            .Include(i => i.Department)
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.InstructorId == profileId);

        if (instructor == null)
        {
            TempData["ErrorMessage"] = "instructor is Null. Please login.";
            return RedirectToAction("LoginUsers","Login");
        }

        var classCount = await _context.Teaches
            .CountAsync(t => t.InstructorId == profileId);

        var studentCount = await _context.Takes
            .Where(t => t.Sections.Teaches.InstructorId == profileId)
            .Select(t => t.StudentId)
            .Distinct()
            .CountAsync();
        return View(new InstructorDashboardViewModel {
            Instructor = instructor,
            ClassCount = classCount,
            StudentCount = studentCount
        });
    }

    public async Task<IActionResult> ClassList()
    {
        var role = HttpContext.Session.GetString("Role");
        var profileId = HttpContext.Session.GetInt32("ProfileId");

        if (role != "Instructor" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var instructor = await _context.Instructors
            .Include(i => i.Department)
            .Include(i => i.User)
            .Include(i => i.Teaches)
            .FirstOrDefaultAsync(i => i.InstructorId == profileId);

        if (instructor == null)
        {
            TempData["ErrorMessage"] = "instructor is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var sections = await _context.Sections
            .Include(s => s.Course)
            .Include(s => s.ClassRoom)
            .Include(s => s.TimeSlot)
            .Include(s => s.Teaches)
            .Include(s => s.Takes)
            .Where(s => s.Teaches.InstructorId == profileId)
            .ToListAsync();

        return View(sections);
    }

    public async Task<IActionResult> StudentsInSection(int? id)
    {
        var role = HttpContext.Session.GetString("Role");
        var profileId = HttpContext.Session.GetInt32("ProfileId");

        if (role != "Instructor" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var instructor = await _context.Instructors
            .Include(i => i.Department)
            .Include(i => i.User)
            .Include(i => i.Teaches)
            .FirstOrDefaultAsync(i => i.InstructorId == profileId);

        if (instructor == null)
        {
            TempData["ErrorMessage"] = "instructor is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        if (id == null)
        {
            TempData["ErrorMessage"] = "Section IDd is missing.";
            return RedirectToAction("ClassList");
        }

        var section = await _context.Sections
            .Include(s => s.Takes)
                .ThenInclude(t => t.Student)
            .Include(s => s.Teaches)
            .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.SectionsId == id && s.Teaches.InstructorId == profileId);

        if (section == null)
        {
            TempData["ErrorMessage"] = "You do not have access to this section.";
            return RedirectToAction("ClassList");
        }

        var takesList = await _context.Takes
            .Include(t => t.Student)
                .ThenInclude(s => s.Department)
            .Include(t => t.Student)
                .ThenInclude(s => s.User)
            .Where(t => t.SectionId == id)
            .ToListAsync();

        ViewBag.CouserName = section.Course.Title;
        ViewBag.Code = section.Code;

        return View(takesList);

    }

    public async Task<IActionResult> RemoveStudent(int? takeId)
    {
        var profileId = HttpContext.Session.GetInt32("ProfileId");
        var role = HttpContext.Session.GetString("Role");

        if (role != "Instructor" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var instructor = await _context.Instructors
            .Include(i => i.Department)
            .Include(i => i.User)
            .Include(i => i.Teaches)
            .FirstOrDefaultAsync(i => i.InstructorId == profileId);

        if (instructor == null)
        {
            TempData["ErrorMessage"] = "instructor is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        if (takeId == null)
        {
            TempData["ErrorMessage"] = "Take ID is missing.";
            return RedirectToAction("ClassList");
        }

        var takes = _context.Takes
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Course)
                .Include(t => t.Student)
                .FirstOrDefault(t => t.TakesId == takeId);
        if (takes == null)
        {
            TempData["ErrorMessage"] = "Take is Not Found.";
            return RedirectToAction("ClassList");
        }

        var sectionTitle = takes.Sections.Code;

        var result = await _InstructorService.RemoveStudent(takeId);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction("ClassList");
        }

        var SendUnassignStudent = await _ChateService.SendMessage(new RoleMessage
        {
            Subject = "UnassignStudent",
            Content = "you Unassign from section with Code : " + sectionTitle,
            SenderId = instructor.UserId,
            ReceiverId = takes.Student.UserId,
        });

        return RedirectToAction("ClassList");
    }

    public async Task<IActionResult> AssignGrade(int takeId)
    {
        var profileId = HttpContext.Session.GetInt32("ProfileId");
        var role = HttpContext.Session.GetString("Role");

        if (role != "Instructor" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var instructor = await _context.Instructors
            .Include(i => i.Department)
            .Include(i => i.User)
            .Include(i => i.Teaches)
            .FirstOrDefaultAsync(i => i.InstructorId == profileId);

        if (instructor == null)
        {
            TempData["ErrorMessage"] = "instructor is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        if (takeId == null)
        {
            TempData["ErrorMessage"] = "Take ID is missing.";
            return RedirectToAction("ClassList");
        }

        var takes = _context.Takes
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Course)
                .Include(t => t.Student)
                    .ThenInclude(s => s.User)
                .Include(t => t.Student)
                    .ThenInclude(s => s.Department)
                .FirstOrDefault(t => t.TakesId == takeId);
        if (takes == null)
        {
            TempData["ErrorMessage"] = "Take takes is missing.";
            return RedirectToAction("ClassList");
        }

        ViewBag.firstName = takes.Student.User.first_name;
        ViewBag.lastName = takes.Student.User.last_name;
        ViewBag.courseName = takes.Sections.Course.Title;
        ViewBag.DepartmentName = takes.Student.Department.Name;
        ViewBag.Id = takeId;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AssignGrade(int takeId, int grade)
    {
        var profileId = HttpContext.Session.GetInt32("ProfileId");
        var role = HttpContext.Session.GetString("Role");

        if (role != "Instructor" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var instructor = await _context.Instructors
            .Include(i => i.Department)
            .Include(i => i.User)
            .Include(i => i.Teaches)
            .FirstOrDefaultAsync(i => i.InstructorId == profileId);

        if (instructor == null)
        {
            TempData["ErrorMessage"] = "instructor is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var takes = await _context.Takes.Include(t => t.Sections).FirstOrDefaultAsync(t => t.TakesId == takeId);
        if (takes == null)
        {
            TempData["ErrorMessage"] = "Record not found.";
            return RedirectToAction("ClassList");
        }

        if (grade < 0 || grade > 20)
        {
            TempData["ErrorMessage"] = "Grade must be between 0 and 20.";
            return RedirectToAction("AssignGrade", new { takeId });
        }


        takes.grade = grade;
        _context.Takes.Update(takes);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Grade assigned successfully.";
        return RedirectToAction("StudentsInSection", new { id = takes.Sections.SectionsId});
    }


    public async Task<IActionResult> SendMessage()
    {
        var profileId = HttpContext.Session.GetInt32("ProfileId");
        var role = HttpContext.Session.GetString("Role");

        if (role != "Instructor" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var instructor = await _context.Instructors
            .Include(i => i.Department)
            .Include(i => i.User)
            .Include(i => i.Teaches)
            .FirstOrDefaultAsync(i => i.InstructorId == profileId);

        if (instructor == null)
        {
            TempData["ErrorMessage"] = "instructor is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var Users = await _context.Users.Where(u => u.Id != instructor.UserId).ToListAsync();
        ViewBag.SenderId = instructor.UserId;
        ViewBag.Receivers = Users;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(RoleMessage message)
    {
        var profileId = HttpContext.Session.GetInt32("ProfileId");
        var role = HttpContext.Session.GetString("Role");

        if (role != "Instructor" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var instructor = await _context.Instructors
            .Include(i => i.Department)
            .Include(i => i.User)
            .Include(i => i.Teaches)
            .FirstOrDefaultAsync(i => i.InstructorId == profileId);

        if (instructor == null)
        {
            TempData["ErrorMessage"] = "instructor is Null. Please login.";
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
        var profileId = HttpContext.Session.GetInt32("ProfileId");
        var role = HttpContext.Session.GetString("Role");

        if (role != "Instructor" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var instructor = await _context.Instructors
            .Include(i => i.Department)
            .Include(i => i.User)
            .Include(i => i.Teaches)
            .FirstOrDefaultAsync(i => i.InstructorId == profileId);

        if (instructor == null)
        {
            TempData["ErrorMessage"] = "instructor is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var userId = instructor.UserId;

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

        var profileId = HttpContext.Session.GetInt32("ProfileId");
        var role = HttpContext.Session.GetString("Role");

        if (role != "Instructor" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var instructor = await _context.Instructors
            .Include(i => i.Department)
            .Include(i => i.User)
            .Include(i => i.Teaches)
            .FirstOrDefaultAsync(i => i.InstructorId == profileId);

        if (instructor == null)
        {
            TempData["ErrorMessage"] = "instructor is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var userId = instructor.UserId;

        if (string.IsNullOrEmpty(withEmail))
        {
            TempData["ErrorMessage"] = "email is Null";
            return RedirectToAction("MessageList");
        }

        var user = await _context.Users.FindAsync(userId);
        var otherUser = await _context.Users.FirstOrDefaultAsync(u => u.email.ToLower() == withEmail.ToLower());
        if (user == null)
        {
            TempData["ErrorMessage"] = "User is Null";
            return RedirectToAction("MessageList");
        }
        if (otherUser == null)
        {
            TempData["ErrorMessage"] = "other User is Null";
            return RedirectToAction("MessageList");
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

    public async Task<IActionResult> AppealList()
    {
        var profileId = HttpContext.Session.GetInt32("ProfileId");
        var role = HttpContext.Session.GetString("Role");

        if (role != "Instructor" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var instructor = await _context.Instructors
            .Include(i => i.Department)
            .Include(i => i.User)
            .Include(i => i.Teaches)
            .FirstOrDefaultAsync(i => i.InstructorId == profileId);

        if (instructor == null)
        {
            TempData["ErrorMessage"] = "instructor is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var userId = instructor.UserId;

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
        var profileId = HttpContext.Session.GetInt32("ProfileId");
        var role = HttpContext.Session.GetString("Role");

        if (role != "Instructor" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var instructor = await _context.Instructors
            .Include(i => i.Department)
            .Include(i => i.User)
            .Include(i => i.Teaches)
            .FirstOrDefaultAsync(i => i.InstructorId == profileId);

        if (instructor == null)
        {
            TempData["ErrorMessage"] = "instructor is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var userId = instructor.UserId;

        if (string.IsNullOrEmpty(withEmail))
        {
            TempData["ErrorMessage"] = "email is Null";
            return RedirectToAction("AppealList");
        }

        var user = await _context.Users.FindAsync(userId);
        var otherUser = await _context.Users.FirstOrDefaultAsync(u => u.email.ToLower() == withEmail.ToLower());
        if (user == null)
        {
            TempData["ErrorMessage"] = "User is Null";
            return RedirectToAction("AppealList");
        }
        if (otherUser == null)
        {
            TempData["ErrorMessage"] = "Other is Null";
            return RedirectToAction("AppealList");
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


    public async Task<IActionResult> SendObjection()
    {
        var profileId = HttpContext.Session.GetInt32("ProfileId");
        var role = HttpContext.Session.GetString("Role");

        if (role != "Instructor" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var instructor = await _context.Instructors
            .Include(i => i.Department)
            .Include(i => i.User)
            .Include(i => i.Teaches)
            .FirstOrDefaultAsync(i => i.InstructorId == profileId);

        if (instructor == null)
        {
            TempData["ErrorMessage"] = "instructor is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var instructorId = instructor.InstructorId;

        var Users = await _context.Takes
            .Include(t => t.Student)
                .ThenInclude(s => s.User)
            .Include(t => t.Sections)
                .ThenInclude(s => s.Teaches)
            .Where(t => t.Sections.Teaches.InstructorId == instructorId && t.Student.DepartmentId == instructor.DepartmentId)
            .Select(t => t.Student.User)
            .Distinct()
            .ToListAsync();
        ViewBag.SenderId = instructor.UserId;
        ViewBag.Receivers = Users;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SendObjection(RoleMessage message)
    {
        var profileId = HttpContext.Session.GetInt32("ProfileId");
        var role = HttpContext.Session.GetString("Role");

        if (role != "Instructor" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var instructor = await _context.Instructors
            .Include(i => i.Department)
            .Include(i => i.User)
            .Include(i => i.Teaches)
            .FirstOrDefaultAsync(i => i.InstructorId == profileId);

        if (instructor == null)
        {
            TempData["ErrorMessage"] = "instructor is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        if (!ModelState.IsValid)
        {
            return View(message);
        }

        var result = await _ChateService.SendMessage(message);

        TempData["SuccessMessage"] = result.Message;

        return RedirectToAction("AppealList");
    }
}
