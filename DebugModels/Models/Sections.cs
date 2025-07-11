using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebugModels.Models
{
    public class Sections
    {
        public int SectionsId { get; set; }
        public int Semester {  get; set; }
        public int year {  get; set; }
        public DateTime final_exam_date { get; set; }
        
        [StringLength(255)]
        public string Description { get; set; } = null!;


        [StringLength(255)]
        [Required]
        public string Code { get; set; } = null!;

        #region
        public int? TeachesId { get; set; }
        [ForeignKey("TeachesId")]
        public Teaches? Teaches { get; set; }

        public List<Takes> Takes { get; set; }

        public int? ClassRoomId { get; set; }
        [ForeignKey("ClassRoomId")]
        public ClassRoom? ClassRoom { get; set;}

        public int? TimeSlotId { get; set; }
        [ForeignKey("TimeSlotId")]
        public TimeSlot? TimeSlot { get; set; }

        public int? CourseId { get;set; }
        [ForeignKey("CourseId")]
        public Course? Course { get; set; }
        #endregion
    }
}
