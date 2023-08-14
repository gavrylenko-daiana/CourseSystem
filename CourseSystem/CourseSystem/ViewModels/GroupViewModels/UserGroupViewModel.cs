using Core.Models;

namespace UI.ViewModels;

public class UserGroupViewModel
{
    public Group Group { get; set; }
    public List<UserGroups> UserGroupsWithoutAdmins { get; set; }
    public AppUser CurrentUser { get; set; }
    public string Progress { get; set; }    
}