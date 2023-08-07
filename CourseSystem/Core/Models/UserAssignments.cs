using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class UserAssignments : BaseEntity
    {
        public string AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; }
        public int AssignmentId { get; set; }
        public virtual Assignment Assignment { get; set; }
        public int Grade { get; set; }
        public bool IsChecked { get; set; }
        public virtual List<AssignmentAnswer> AssignmentAnswers { get; set; }
    }
}
