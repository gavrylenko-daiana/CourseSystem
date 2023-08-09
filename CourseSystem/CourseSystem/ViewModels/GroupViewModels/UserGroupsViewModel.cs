using Core.Models;

namespace UI.ViewModels;

public class UserGroupsViewModel
{
    public List<GroupViewModel> Groups { get; set; }
    public AppUser CurrentUser { get; set; }
}