﻿using Core.Enums;

namespace UI.ViewModels.EmailViewModels
{
    public class ConfirmEmailViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public AppUserRoles Role { get; set; }
        public string UserId { get; set; }
    }
}
