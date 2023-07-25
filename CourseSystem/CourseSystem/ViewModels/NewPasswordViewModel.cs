using System.ComponentModel.DataAnnotations;

namespace UI.ViewModels;

public class NewPasswordViewModel
{
    [Required(ErrorMessage = "New Password is required")]
    [Display(Name = "New Password")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }
    
    [Required(ErrorMessage = "Confirm New Password is required")]
    [Display(Name = "Confirm New Password")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Password do not match")]
    public string ConfirmNewPassword { get; set; }
    
    public string Email { get; set; }
}