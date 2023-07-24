using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Group : BaseEntity
    {
        public string Name { get; set; }
        //public double Progress { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
        public virtual List<EducationMaterial> EducationMaterials { get; set; }
        public virtual List<Assignment> Assignments { get; set; }
        public virtual List<UserGroups> UserGroups { get; set; }
    }
}
