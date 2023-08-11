using System.ComponentModel.DataAnnotations;
using Core.Enums;
using Core.Models;

namespace UI.ViewModels;

public class CreateInCourseEducationMaterialViewModel
{
    [Required(ErrorMessage = "Uploaded file is required")]
    [Display(Name = "Upload File")]
    public IFormFile UploadFile { get; set; }
    
    [Required(ErrorMessage = "Material access is required")]
    [Display(Name = "Material access")]
    public MaterialAccess MaterialAccess { get; set; }
    
    public Course Course { get; set; }
    public List<Group> Groups { get; set; }
    public int SelectedGroupId { get; set; }
    public int CourseId { get; set; }
}