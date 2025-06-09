using System.ComponentModel.DataAnnotations;

namespace DebugModels.Models
{
    public class User
    {
        public int Id { get; set; } 
        public DateTime created_at { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string first_name { get; set; } = null!;

        [StringLength(50)]
        public string last_name { get; set;} = null!;

        [EmailAddress]
        public string email { get; set; } = null!;

        [StringLength(50,MinimumLength =6)]
        public string hashed_password { get; set; } = null!;

        public List<UserRole> UserRoles {  get; set; }

        public List<Instructor> Instructors { get; set; }
        
        public List<Student> Students { get; set; } 
    }
}
