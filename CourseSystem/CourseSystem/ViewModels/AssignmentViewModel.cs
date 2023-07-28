using Core.Enums;
using Core.Models;

namespace UI.ViewModels
{
    public class AssignmentViewModel
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AssignmentAccess AssignmentAccess { get; set; }
        public List<UserAssignments> UserAssignments { get; set; }
    }
}
