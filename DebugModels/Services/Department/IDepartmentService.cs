using DebugModels.Utils;

namespace DebugModels.Services.Department
{
    public interface IDepartmentService
    {
        Task<OperationResult> DeleteDepartment(int departmentId);
    }
}
