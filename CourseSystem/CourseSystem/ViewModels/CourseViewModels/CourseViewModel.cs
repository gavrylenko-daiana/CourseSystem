using Core.Models;

namespace UI.ViewModels.CourseViewModels;

public class CourseViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public IFormFile UploadImage { get; set; }
    public CourseBackgroundImage BackgroundImage { get; set; }
    public List<Group> CurrentGroups { get; set; }
    public List<EducationMaterial> EducationMaterials { get; set; }
    public List<UserCourses> UserCoursesWithoutAdmins { get; set; }
    public AppUser CurrentUser { get; set; }
}