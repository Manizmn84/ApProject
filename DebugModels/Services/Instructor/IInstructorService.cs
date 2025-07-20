using DebugModels.Utils;

namespace DebugModels.Services.Instructor
{
    public interface IInstructorService
    {
        Task<OperationResult> DeleteInstructor(int InstructorId);
        Task<OperationResult> RemoveStudent(int? takeId);
    }
}
