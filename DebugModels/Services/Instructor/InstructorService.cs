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
    }
}
