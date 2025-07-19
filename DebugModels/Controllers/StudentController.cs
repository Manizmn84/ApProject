using DebugModels.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class StudentController : Controller
{
    private readonly ProjectContext _context;

    public StudentController(ProjectContext context)
    {
        _context = context;
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
            .Include(i => i.Department)
            .FirstOrDefault(s => s.StudentId== profileId);

        if (student == null)
        {
            TempData["ErrorMessage"] = "student is Null. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        return View(student);
    }
}
