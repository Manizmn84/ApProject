using DebugModels.Data;
using DebugModels.Models.ViewModels;
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



}
