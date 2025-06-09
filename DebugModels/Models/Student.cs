using System.ComponentModel.DataAnnotations.Schema;

namespace DebugModels.Models
{
    public class Student
    {
        public int StudentId { get; set; }
        public DateTime enrollment_date { get; set; }

        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public List<Takes> Takes { get; set; }
    }
}
