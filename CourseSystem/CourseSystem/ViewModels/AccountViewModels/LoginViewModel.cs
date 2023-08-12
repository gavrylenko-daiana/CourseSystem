using System.ComponentModel.DataAnnotations;

namespace UI.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email Address is required")]
    [Display(Name = "• Email Address •")]
    [DataType(DataType.EmailAddress)]
    public string EmailAddress { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    [Display(Name = "• Password •")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}