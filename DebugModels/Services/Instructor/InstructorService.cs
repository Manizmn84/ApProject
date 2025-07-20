using DebugModels.Data;
using DebugModels.Models;
using DebugModels.Utils;
using Microsoft.EntityFrameworkCore;

namespace DebugModels.Services.Instructor
{
    public class InstructorService : IInstructorService
    {
        private readonly ProjectContext _context;

        public InstructorService(ProjectContext context)
        {
            _context = context;
        }

        public async Task<OperationResult> DeleteInstructor(int InstructorId)
        {
            var Instructor = await _context.Instructors.Include(In => In.Teaches).Include(In => In.User).FirstOrDefaultAsync(In => In.InstructorId == InstructorId);

            if (Instructor == null)
                return OperationResult.Fail($"Don`t have any Instructor With that Id {InstructorId}");
            var userId = Instructor.UserId;
            var user  = await _context.Users.Include(u => u.Instructors).Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == userId);

            if (Instructor.Teaches != null && Instructor.Teaches.Count > 0)
                _context.Teaches.RemoveRange(Instructor.Teaches);

            _context.Instructors.Remove(Instructor);

            if (user.Instructors == null || !user.Instructors.Any(i => i.InstructorId != InstructorId))
            {
                var InstructorRole = user.UserRoles.FirstOrDefault(r => r.Role.name == "Instructor");
                if (InstructorRole != null)
                    _context.UserRoles.Remove(InstructorRole);
            }

            await _context.SaveChangesAsync();

            return OperationResult.Ok("Instructor deleted successfully.");
        }

        public async Task<OperationResult> RemoveStudent(int? takeId)
        {
            var takes = _context.Takes
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Course)
                .Include(t => t.Student)
                .FirstOrDefault(t => t.TakesId == takeId);

            if (takes == null)
            {
                return OperationResult.Fail("Student assignment not found.");
            }

            var studentId = takes.StudentId;
            var courseId = takes.Sections?.Course?.CourseId;

            if (courseId == null)
            {
                return OperationResult.Fail("Course information is missing.");
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
                    return OperationResult.Fail("Cannot unassign this student. This course is a prerequisite for another course the student is currently taking.");
                }
            }


            _context.Takes.Remove(takes);
            _context.SaveChanges();

            return OperationResult.Ok("Student unassigned from section successfully.");
        }
    }
}
