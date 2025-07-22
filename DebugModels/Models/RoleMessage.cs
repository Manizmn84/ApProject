using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DebugModels.Models
{
    public class RoleMessage
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Subject { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;

        public int? SenderId { get; set; }
        [ForeignKey("SenderId")]
        public User? Sender { get; set; }

        public int? ReceiverId { get; set; }
        [ForeignKey("ReceiverId")]
        public User? Receiver { get; set; }
    }

}
