using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class AppUser : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime BirthDate { get; set; }
        public string University { get; set; }
        public string Telegram { get; set; }
        public string GitHub { get; set; }
        public AppUserRoles Role { get; set; }
        public virtual List<UserAssignments> UserAssignments { get; set; }
        public virtual List<UserCourses> UserCourses { get; set;}
        public virtual List<UserGroups> UserGroups { get; set; }
    }
}
