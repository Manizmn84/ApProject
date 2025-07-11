using System.ComponentModel.DataAnnotations.Schema;

namespace DebugModels.Models
{
    public class Takes
    {
        public int TakesId { get; set; }
        public int grade {  get; set; }

        #region
        public int? StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        public int? SectionId { get; set; }
        [ForeignKey("SectionId")]
        public Sections? Sections { get; set; }
        #endregion
    }
}
