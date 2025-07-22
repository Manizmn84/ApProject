using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DebugModels.Models.ViewModels
{
    public class AppealView
    {
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }

        public int sectionId { get; set; }
    }
}
