using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Course : BaseEntity
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public virtual List<Group> Groups { get; set; }
        public virtual List<EducationMaterial> EducationMaterials { get; set; }
        public virtual List<UserCourses> UserCourses { get; set; }
        public virtual List<Notification> Notifications { get; set; }
    }
}