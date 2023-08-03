using Core.Models;

namespace UI.ViewModels;

public class CourseViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Group> CurrentGroups { get; set; }
    public List<EducationMaterial> EducationMaterials { get; set; }
    public List<UserCourses> UserCourses { get; set; }
    public AppUser CurrentUser { get; set; }
}