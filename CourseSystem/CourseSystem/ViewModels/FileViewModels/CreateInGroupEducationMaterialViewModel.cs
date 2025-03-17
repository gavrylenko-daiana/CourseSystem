using System.ComponentModel.DataAnnotations;
using Core.Enums;
using Core.Models;

namespace UI.ViewModels.FileViewModels;

public class CreateInGroupEducationMaterialViewModel
{
    [Required(ErrorMessage = "Uploaded file is required")]
    [Display(Name = "Upload File")]
    public IFormFile UploadFile { get; set; }
    
    [Required(ErrorMessage = "Material access is required")]
    [Display(Name = "Material access")]
    public MaterialAccess MaterialAccess { get; set; }
    
    public DateTime TimeUploaded { get; set; }
    
    public Group? Group { get; set; }
    public int GroupId { get; set; }
    public Course? Course { get; set; }
    public int CourseId { get; set; }
}