using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace DebugModels.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Department name is required")]
        [MaxLength(50, ErrorMessage = "Department name cannot exceed 50 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Building is required")]
        public string Building { get; set; }

        [Required(ErrorMessage = "Budget is required")]
        [Range(0, 99999999.99, ErrorMessage = "Budget cannot exceed 99,999,999.99")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Budget { get; set; }


        #region
        public List<Instructor>? Instructors { get; set; }

        public List<Student>? Students { get; set; }

        public List<Course>? Courses { get; set; }
        #endregion
    }
}
