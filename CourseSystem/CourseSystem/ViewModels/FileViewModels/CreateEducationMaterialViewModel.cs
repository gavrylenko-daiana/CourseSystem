using System.ComponentModel.DataAnnotations;
using Core.Enums;

namespace UI.ViewModels;

public class CreateEducationMaterialViewModel
{
    [Required(ErrorMessage = "Uploaded file is required")]
    [Display(Name = "Upload File")]
    public IFormFile UploadFile { get; set; }
    
    [Required(ErrorMessage = "Material access is required")]
    [Display(Name = "Material access")]
    public MaterialAccess MaterialAccess { get; set; }
    
    public int CourseId { get; set; }
    public int GroupId { get; set; }
}