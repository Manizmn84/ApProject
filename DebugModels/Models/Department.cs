using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace DebugModels.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        [Required]
        public string Name { get; set; }

        public string Building { get; set; }

        public decimal Budget { get; set; }

        #region
        public List<Instructor> Instructors { get; set; }

        public List<Student> Students { get; set; }

        public List<Course> Courses { get; set; }
        #endregion
    }
}
