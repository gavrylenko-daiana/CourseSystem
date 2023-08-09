using Core.Enums;
using Core.Models;
using System.ComponentModel.DataAnnotations;


namespace UI.ViewModels.AssignmentViewModels
{
    public class CreateAssignmentViewModel //add property for markdown text
    {
        [Required(ErrorMessage = "Assignment name is required")]
        [Display(Name = "* Assignment name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Assignment description is required")]
        [Display(Name = "* Assignment description")]
        public string Description { get; set; }

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
        public List<IFormFile>? AttachedFiles { get; set; }
    }
}
