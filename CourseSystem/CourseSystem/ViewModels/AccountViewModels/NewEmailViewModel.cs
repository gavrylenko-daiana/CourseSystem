using System.ComponentModel.DataAnnotations;

namespace UI.ViewModels;

public class NewEmailViewModel
{
    [Required(ErrorMessage = "New Email is required")]
    [Display(Name = "New Email")]
    [DataType(DataType.EmailAddress)]
    public string NewEmail { get; set; }
    public string Email { get; set; }
}