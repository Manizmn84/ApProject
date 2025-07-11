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
        public string Unit { get; set; } = null!;

        #region
        public int? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        public List<Sections>? Sections { get; set; }

        public List<PreRegs>? PreRegs { get; set; }
        #endregion
    }
}

