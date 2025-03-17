﻿using Core.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? University { get; set; }
        public string? Telegram { get; set; }
        public string? GitHub { get; set; }
        public AppUserRoles Role { get; set; }
        public virtual ProfileImage ProfileImage { get; set; }
        public virtual List<UserAssignments> UserAssignments { get; set; }
        public virtual List<UserCourses> UserCourses { get; set; }
        public virtual List<UserGroups> UserGroups { get; set; }
        public virtual List<UserActivity> UserActivities { get; set; }
        public virtual List<Notification> Notifications { get; set; }
        public virtual List<ChatMessage> ChatMessages { get; set; }
    }
}