using Core.Models;

namespace UI.ViewModels;

public class TeacherViewModel
{
    public string Id { get; set; }
    public int CourseId { get; set; }
    public int GroupId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsInvited { get; set; }
    public ProfileImage ProfileImage { get; set; }
}