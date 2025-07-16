using DebugModels.Utils;

namespace DebugModels.Services.Student
{
    public interface IStudentService
    {
        Task<OperationResult> DeleteStudent(int StudentId);
    }
}
