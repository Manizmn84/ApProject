using System.ComponentModel.DataAnnotations;

namespace DebugModels.Models
{
    public class ClassRoom
    {
        public int ClassRoomId { get; set; }
        [StringLength(255)]
        public string buliding { set; get; } = null!;
        public int RoomNumber { get; set; }
        public int Capacity { get; set; }
        
        #region
        public List<Sections>? Sections { get; set; }
        #endregion
    }
}
