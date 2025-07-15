using DebugModels.Utils;

namespace DebugModels.Services.Course
{
    public interface ICourseService
    {
        Task<OperationResult> DeleteCourse(int courseId);
    }
}
