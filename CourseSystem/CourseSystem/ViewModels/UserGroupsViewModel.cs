using Core.Models;

namespace UI.ViewModels;

public class UserGroupsViewModel
{
    public List<Group> Groups { get; set; }
    public AppUser CurrentUser { get; set; }
}