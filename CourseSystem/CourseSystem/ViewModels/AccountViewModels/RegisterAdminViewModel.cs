using System.ComponentModel.DataAnnotations;
using Core.Enums;

namespace UI.ViewModels;

public class RegisterAdminViewModel
{
    [Required(ErrorMessage = "FirstName is required")]
    [Display(Name = "• FirstName •")]
    public string FirstName { get; set; }
    
    [Required(ErrorMessage = "LastName is required")]
    [Display(Name = "• LastName •")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email Address is required")]
    [Display(Name = "• Email Address •")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    public AppUserRoles Role { get; set; } = AppUserRoles.Admin;
}