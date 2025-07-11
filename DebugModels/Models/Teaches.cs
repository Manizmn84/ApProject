using System.ComponentModel.DataAnnotations.Schema;

namespace DebugModels.Models
{
    public class Teaches
    {
        public int TeachesId { get; set; }

        #region
        public int? InstructorId { get; set; }
        [ForeignKey("InstructorId")]
        public Instructor? Instructor { get; set; }

        public Sections? Sections { get; set; }
        #endregion
    }
}
