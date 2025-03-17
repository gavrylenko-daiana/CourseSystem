using System.ComponentModel.DataAnnotations;
using Core.Enums;

namespace UI.ViewModels;

public class ForgotViewModel
{
    [Display(Name = "Email Address")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
    
    [Display(Name = "Code from Mail")]
    public int EmailCode { get; set; }
    public ForgotEntity ForgotEntity { get; set; }
}