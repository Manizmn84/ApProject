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
            var Instructor = await _context.Instructors.Include(In => In.Teaches).FirstOrDefaultAsync(In => In.InstructorId == InstructorId);

            if (Instructor == null)
                return OperationResult.Fail($"Don`t have any Instructor With that Id {InstructorId}");

            if (Instructor.Teaches != null && Instructor.Teaches.Count > 0)
                _context.Teaches.RemoveRange(Instructor.Teaches);

            _context.Instructors.Remove(Instructor);
            await _context.SaveChangesAsync();

            return OperationResult.Ok("Instructor deleted successfully.");
        }
    }
}
