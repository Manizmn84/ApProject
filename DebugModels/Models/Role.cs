using System.ComponentModel.DataAnnotations;

namespace DebugModels.Models
{
    public class Role
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string name { get; set; } = null!;

        #region
        public List<UserRole>? UserRoles { get; set; }
        #endregion
    }
}
