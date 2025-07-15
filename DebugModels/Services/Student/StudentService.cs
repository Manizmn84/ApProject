using DebugModels.Data;
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

            if(student.Takes != null && student.Takes.Count > 0)
                _context.Takes.RemoveRange(student.Takes);

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return OperationResult.Ok($"Delete Student With ID : {StudentId} is Successfully");
        }
    }
}
