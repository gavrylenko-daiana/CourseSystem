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
    public string EmailAddress { get; set; }

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
    
    [Required(ErrorMessage = "Date of Birthday is required")]
    [Display(Name = "* Date of Birthday")]
    [DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }
    
    [Required(ErrorMessage = "University is required")]
    [Display(Name = "* University")]
    public string University { get; set; }
    
    [Display(Name = "Telegram")]
    public string? Telegram { get; set; }
    
    [Display(Name = "GitHub")]
    public string? GitHub { get; set; }
}