using Core.Models;

namespace UI.ViewModels;

public class UserCoursesViewModel
{
    public AppUser CurrentUser { get; set; }
    public List<Course> Courses { get; set; }
}