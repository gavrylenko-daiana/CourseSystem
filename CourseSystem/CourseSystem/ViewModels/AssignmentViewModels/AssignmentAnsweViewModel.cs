using System.ComponentModel.DataAnnotations;


namespace UI.ViewModels.AssignmentViewModels
{
    public class AssignmentAnsweViewModel
    {
        public int AssignmentId { get; set; }

        [Display(Name = "Answer description")]
        public string? AnswerDescription { get; set; }
        public List<IFormFile>? AssignmentAnswerFiles { get; set; }

    }
}
