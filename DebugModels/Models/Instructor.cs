using System.ComponentModel.DataAnnotations.Schema;

namespace DebugModels.Models
{
    public class Instructor
    {
        public int InstructorId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }
        public DateTime hire_date { set; get; }

        #region relation
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public List<Teaches>? Teaches { get; set; }
        #endregion
    }
}
