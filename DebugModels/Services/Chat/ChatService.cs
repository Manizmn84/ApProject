using DebugModels.Data;
using DebugModels.Models;
using DebugModels.Utils;

namespace DebugModels.Services.Chat
{
    public class ChatService : IChatService
    {
        private readonly ProjectContext _context;

        public ChatService(ProjectContext context)
        {
            _context = context;
        }

        public async Task<OperationResult> SendMessage(RoleMessage message)
        {
            _context.RoleMessages.Add(message);
            await _context.SaveChangesAsync();

            return OperationResult.Ok("Message sent successfully.");
        }

    }
}