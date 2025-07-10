using System.ComponentModel.DataAnnotations.Schema;

namespace DebugModels.Models
{
    public class PreRegs
    {
        public int Id { get; set; }


        #region
        public int? PreRegCourseId { get; set; }
        [ForeignKey("PreRegCourseId")]
        public Course? PreRegCourse { get; set; }

        public int? CoureId { get; set; }
        [ForeignKey("CoureId")]
        public Course Course { get; set; }
        #endregion

    }
}
