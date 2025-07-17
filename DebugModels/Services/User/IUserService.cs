using Azure;
using DebugModels.Models.ViewModels;
using DebugModels.Utils;

namespace DebugModels.Services.User
{
    public interface IUserService
    {
        Task<OperationResult> LoginUser(LoginUserViewModel model);
    }
}
