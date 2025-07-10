using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebugModels.Models
{
    public class Course
    {
        public int CourseId { get; set; }

        [StringLength(255)]
        public string Title { get; set; } = null!;

        [StringLength(255)]
        [Required]
        public string Code { get; set; } = null!;

        [StringLength(255)]
        public string Unit { get; set; } = null!;

        [StringLength(255)]
        public string Description { get; set; } = null!;

        public DateTime final_exam_date { get; set; }

        public int? SectionsId { get; set; }

        [ForeignKey("SectionsId")]
        public Sections? Sections { get; set; }

    }
}

