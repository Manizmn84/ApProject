using DebugModels.Models;
using DebugModels.Utils;

namespace DebugModels.Services.Chat
{
    public interface IChatService
    {
        Task<OperationResult> SendMessage(RoleMessage message);
    }
}
