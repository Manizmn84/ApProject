using System.ComponentModel.DataAnnotations.Schema;

namespace DebugModels.Models
{
    public class Sections
    {
        public int SectionsId { get; set; }
        public int Semester {  get; set; }
        public int year {  get; set; }
        public int? TeachesId { get; set; }
        [ForeignKey("TeachesId")]
        public Teaches? Teaches { get; set; }

        public int? TakesId { get; set; }
        [ForeignKey("TakesId")]
        public Takes? Takes { get; set; }

        public Course? Course { get; set; }

        public int? ClassRoomId { get; set; }
        [ForeignKey("ClassRoomId")]
        public ClassRoom? ClassRoom { get; set;}

        public int? TimeSlotId { get; set; }
        [ForeignKey("TimeSlotId")]
        public TimeSlot? TimeSlot { get; set; }
    }
}
