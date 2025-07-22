using Microsoft.AspNetCore.Mvc;

namespace DebugModels.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int DepartmentCount { get; set; }
        public int UserCount { get; set; }
        public int InstructorCount { get; set; }
        public int StudentCount { get; set; }
        public int CourseCount { get; set; }
        public int SectionCount { get; set; }
    }
}
