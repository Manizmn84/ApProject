using System.ComponentModel.DataAnnotations;

namespace DebugModels.Models
{
    public class TimeSlot
    {
        public int TimeSlotId { get; set; }
        [StringLength(255)]
        public string Day { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public List<Sections> Sections { get; set; }
    }
}
