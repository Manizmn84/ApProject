namespace DebugModels.Models.ViewModels
{
    public class UserFullInfoViewModel
    {
        public User User { get; set; } = null!;
        public Instructor? Instructor { get; set; }
        public Student? Student { get; set; }
    }
}



