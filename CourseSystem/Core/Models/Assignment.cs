using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Assignment : BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AssignmentAccess AssignmentAccess { get; set; }
        public int GroupId { get; set; }
        public virtual Group Group { get; set; }
        public virtual List<AssignmentFile> AssignmentFiles { get; set; }
        public virtual List<UserAssignments> UserAssignments { get; set; }
        public virtual List<ChatMessage> ChatMessages { get; set; }
        public virtual List<Notification> Notifications { get; set; }
    }
}