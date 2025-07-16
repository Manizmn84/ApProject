using DebugModels.Data;
using DebugModels.Models;
using DebugModels.Utils;
using Microsoft.EntityFrameworkCore;

namespace DebugModels.Services.Student
{
    public class StudentService : IStudentService
    {
        private readonly ProjectContext _context;
        public StudentService(ProjectContext context)
        {
            _context = context;
        }

        public async Task<OperationResult> DeleteStudent(int StudentId)
        {
            var student = await _context.Students.Include(s => s.Takes).FirstOrDefaultAsync(s => s.StudentId == StudentId);

            if (student == null)
                return OperationResult.Fail($"We Don`t have any Student With That ID : {StudentId}");

            var userId = student.UserId;
            var user = await _context.Users.Include(u => u.Students).Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == userId);


            if (student.Takes != null && student.Takes.Count > 0)
                _context.Takes.RemoveRange(student.Takes);

            _context.Students.Remove(student);

            if (user.Students == null || !user.Students.Any(i => i.StudentId!= StudentId))
            {
                var StudentRole = user.UserRoles.FirstOrDefault(r => r.Role.name == "Student");
                if (StudentRole != null)
                    _context.UserRoles.Remove(StudentRole);
            }
            await _context.SaveChangesAsync();

            return OperationResult.Ok($"Delete Student With ID : {StudentId} is Successfully");
        }
    }
}
