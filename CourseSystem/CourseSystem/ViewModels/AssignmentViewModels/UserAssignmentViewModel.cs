using Core.Models;

namespace UI.ViewModels
{
    public class UserAssignmentViewModel
    {
        public int Id { get; set; }
        public  AppUser AppUser { get; set; }
        public  Assignment Assignment { get; set; }
        public List<AssignmentAnswer> AssignmentAnswers { get; set; }
        public int Grade { get; set; }
        public bool IsChecked { get; set; }
    }
}
