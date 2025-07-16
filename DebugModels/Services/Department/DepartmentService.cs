using DebugModels.Data;
using DebugModels.Services.Course;
using DebugModels.Utils;
using Microsoft.EntityFrameworkCore;

namespace DebugModels.Services.Department
{
    public class DepartmentService : IDepartmentService
    {
        private readonly ProjectContext _context;
        private readonly ICourseService _courseService;

        public DepartmentService(ProjectContext context, ICourseService courseService)
        {
            _context = context;
            _courseService = courseService;
        }

        public async Task<OperationResult> DeleteDepartment(int departmentId)
        {
            var department = await _context.Departments
                .Include(d => d.Courses)
                .Include(d => d.Instructors).ThenInclude(In => In.Teaches)
                .Include(d => d.Students).ThenInclude(St => St.Takes)
                .FirstOrDefaultAsync(d => d.Id == departmentId);

            if (department == null)
                return OperationResult.Fail($"Don`t have any Department With That Id {departmentId}");

            foreach (var course in department.Courses.ToList())
            {
                var Result = await _courseService.DeleteCourse(course.CourseId);
                if (!Result.Success)
                    return OperationResult.Fail($"Fail to Delete Course With Id {course.CourseId}");
            }

            foreach (var instructor in department.Instructors)
            {
                if (instructor.Teaches != null && instructor.Teaches.Count > 0)
                    _context.Teaches.RemoveRange(instructor.Teaches);
            }

            foreach (var student in department.Students)
            {
                if (student.Takes != null && student.Takes.Count > 0)
                    _context.Takes.RemoveRange(student.Takes);
            }

            _context.Students.RemoveRange(department.Students);
            _context.Instructors.RemoveRange(department.Instructors);

            _context.Departments.Remove(department);

            await _context.SaveChangesAsync();

            return OperationResult.Ok("Delete Department is successfully");

        }
    }
}
