using DebugModels.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class InstructorController : Controller
{
    private readonly ProjectContext _context;

    public InstructorController(ProjectContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var role = HttpContext.Session.GetString("Role");
        var profileId = HttpContext.Session.GetInt32("ProfileId");

        if (role != "Instructor" || profileId == null)
        {
            TempData["ErrorMessage"] = "Access denied. Please login.";
            return RedirectToAction("LoginUsers", "Login");
        }

        var instructor = _context.Instructors
            .Include(i => i.Department)
            .FirstOrDefault(i => i.InstructorId == profileId);

        if (instructor == null)
        {
            TempData["ErrorMessage"] = "instructor is Null. Please login.";
            return RedirectToAction("LoginUsers","Login");
        }

        return View(instructor);
    }
}
