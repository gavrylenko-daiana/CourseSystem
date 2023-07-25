using System.ComponentModel.DataAnnotations;
using Core.Enums;
using Core.Models;

namespace UI.ViewModels;

// public class UserViewModel
// {
//     public string Id { get; set; }
//     
//     [Required(ErrorMessage = "FirstName is required")]
//     [Display(Name = "FirstName")]
//     public string FirstName { get; set; }
//     
//     [Required(ErrorMessage = "LastName is required")]
//     [Display(Name = "LastName")]
//     public string LastName { get; set; }
//     
//     [Required(ErrorMessage = "Email Address is required")]
//     [Display(Name = "Email Address")]
//     public string Email { get; set; }
//     
//     [Required(ErrorMessage = "Role is required")]
//     [Display(Name = "Role")]
//     public AppUserRoles Role { get; set; }
//     
//     [Required(ErrorMessage = "Date of Birthday is required")]
//     [Display(Name = "Date of Birthday")]
//     [DataType(DataType.Date)]
//     public DateTime BirthDate { get; set; }
//     
//     [Required(ErrorMessage = "University is required")]
//     [Display(Name = "University")]
//     public string University { get; set; }
//     
//     [Display(Name = "Telegram")]
//     public string? Telegram { get; set; }
//     
//     [Display(Name = "GitHub")]
//     public string? GitHub { get; set; }
//     
//     public virtual List<UserAssignments> UserAssignments { get; set; }
//     public virtual List<UserCourses> UserCourses { get; set;}
//     public virtual List<UserGroups> UserGroups { get; set; }
// }