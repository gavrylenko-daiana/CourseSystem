using System.ComponentModel.DataAnnotations;

namespace UI.ViewModels;

public class EditUserViewModel
{
    [Required(ErrorMessage = "FirstName is required")]
    [Display(Name = "FirstName")]
    public string FirstName { get; set; }
    
    [Required(ErrorMessage = "LastName is required")]
    [Display(Name = "LastName")]
    public string LastName { get; set; }
    
    [Display(Name = "Date of Birthday")]
    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }

    [Required(ErrorMessage = "Email Address is required")]
    [Display(Name = "Email Address")]
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }
    
    [Display(Name = "University")]
    public string? University { get; set; }
    
    [Display(Name = "Telegram")]
    public string? Telegram { get; set; }
    
    [Display(Name = "GitHub")]
    public string? GitHub { get; set; }
}