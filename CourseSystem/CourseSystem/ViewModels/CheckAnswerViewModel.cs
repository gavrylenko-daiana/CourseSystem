using Core.Models;

namespace UI.ViewModels
{
    public class CheckAnswerViewModel
    {
        public int Id { get; set; }
        public AppUser AppUser { get; set; }
        public int Grade { get; set; }
        public bool IsChecked { get; set; }
        public List<AssignmentAnswer> AssignmentAnswers { get; set; }
    }
}
