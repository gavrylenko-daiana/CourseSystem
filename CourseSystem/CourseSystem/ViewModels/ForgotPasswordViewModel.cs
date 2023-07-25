using System.ComponentModel.DataAnnotations;

namespace UI.ViewModels;

public class ForgotPasswordViewModel
{
    [Display(Name = "Email Address")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
    
    [Display(Name = "Code from Mail")]
    public int EmailCode { get; set; }
}