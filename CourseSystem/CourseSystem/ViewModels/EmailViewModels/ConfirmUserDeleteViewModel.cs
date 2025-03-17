using Core.Enums;

namespace UI.ViewModels.EmailViewModels
{
    public class ConfirmUserDeleteViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public AppUserRoles Role { get; set; }
    }
}
