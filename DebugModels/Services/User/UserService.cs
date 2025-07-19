using DebugModels.Data;
using DebugModels.Models.ViewModels;
using DebugModels.Utils;
using Microsoft.EntityFrameworkCore;
using YourProjectNamespace.Utilities;

namespace DebugModels.Services.User
{
    public class UserService : IUserService
    {
        private readonly ProjectContext _context;
        public UserService(ProjectContext context)
        {
            _context = context;
        }

        public async Task<OperationResult> LoginUser(LoginUserViewModel model)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles!)
                    .ThenInclude(ur => ur.Role!)
                .FirstOrDefaultAsync(u =>
                    u.email.ToLower() == model.Email.ToLower() &&
                    u.hashed_password == PasswordHelper.HashPassword(model.Password) &&
                    u.UserRoles!.Any(ur => ur.Role!.name == model.Role)
                );

            if (user == null)
                return OperationResult.Fail("We Don`t have User");

            return OperationResult.Ok("We have User");
        }

    }
}
