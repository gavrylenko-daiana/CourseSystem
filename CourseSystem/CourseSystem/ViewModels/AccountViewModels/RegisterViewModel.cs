using System.ComponentModel.DataAnnotations;
using Core.Enums;

namespace UI.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "FirstName is required")]
    [Display(Name = "* FirstName")]
    public string FirstName { get; set; }
    
    [Required(ErrorMessage = "LastName is required")]
    [Display(Name = "* LastName")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email Address is required")]
    [Display(Name = "* Email Address")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [Display(Name = "* Password")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required(ErrorMessage = "Confirm Password is required")]
    [Display(Name = "* Confirm Password")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Password do not match")]
    public string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "Role is required")]
    [Display(Name = "* Role")]
    public AppUserRoles Role { get; set; }
}