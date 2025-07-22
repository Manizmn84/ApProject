using DebugModels.Models;
using System.Collections.Generic;

namespace DebugModels.Models.ViewModels
{
    public class SelectDepartmentViewModel
    {
        public int SelectedDepartmentId { get; set; }
        public List<Department> Departments { get; set; }
    }

}

