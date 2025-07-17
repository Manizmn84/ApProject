using DebugModels.Data;
using DebugModels.Models;
using DebugModels.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class InstructorController : Controller
{
    private readonly ProjectContext _context;

    public InstructorController(ProjectContext context)
    {
        _context = context;
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();

        TempData["SuccessMessage"] = "Logout SuccessFully";


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
            TempData["ErrorMessage"] = "Section ID is missing.";
            return RedirectToAction("ClassList");
        }

        var section = await _context.Sections
            .Include(s => s.Takes)
                .ThenInclude(t => t.Student)
            .Include(s => s.Teaches)
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


        return View(takesList);

    }
}
