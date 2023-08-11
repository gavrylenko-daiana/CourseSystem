using Core.Enums;
using System.ComponentModel.DataAnnotations;


namespace UI.ViewModels.AssignmentViewModels
{
    public class EditAssignmentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Assignment name is required")]
        [Display(Name = "* Assignment name")]
        public string Name { get; set; }]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "* Start date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [Display(Name = "* End date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Assignment status is required")]
        [Display(Name = "* Assignment status")]
        public AssignmentAccess AssignmentAccess { get; set; }
        public int GroupId { get; set; }
        public List<FileCheckBoxViewModel> AttachedFilesCheckBoxes { get; set; } = new List<FileCheckBoxViewModel>();
        public List<string>? AttachedFilesUrls { get; set; }
        public List<IFormFile>? NewAddedFiles { get; set; }

    }
}
