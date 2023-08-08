using Core.Enums;
using Core.Models;

namespace UI.ViewModels.AssignmentViewModels
{
    public class AssignmentViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int GroupId { get; set; }
        public AssignmentAccess AssignmentAccess { get; set; }
        public UserAssignments? UserAssignment { get; set; }
    }
}
