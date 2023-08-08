using Core.Models;

namespace UI.ViewModels
{
    public class FileCheckBoxViewModel
    {
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public AssignmentFile Value { get; set; }
    }
}
