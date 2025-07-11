namespace DebugModels.Models
{
    public class UserRole
    {
        #region
        public int? UserId { get; set; }
        public User? User { get; set; }

        public int? RoleId { get; set; }
        public Role? Role { get; set; }
        #endregion
    }
}
