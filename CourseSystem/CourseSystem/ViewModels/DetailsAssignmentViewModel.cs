using Core.Enums;
using Core.Models;

namespace UI.ViewModels
{
    public class DetailsAssignmentViewModel //property for markdown text
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AssignmentAccess AssignmentAccess { get; set; }
        public  Group Group { get; set; }
        public List<IFormFile> AttachedFiles { get; set; }
        public UserAssignments UserAssignment { get; set; }
        public List<ChatMessage> ChatMessages { get; set; }
    }
}
