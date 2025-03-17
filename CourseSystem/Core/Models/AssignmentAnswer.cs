using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Enums;

namespace Core.Models
{
    public class AssignmentAnswer : BaseEntity
    {
        public string Name { get; set; }
        public string? Text { get; set; }
        public string Url { get; set; }
        public string FileExtension { get; set; }
        public DropboxFolders DropboxFolder { get; set; }
        public DateTime CreationTime { get; set; }
        public int UserAssignmentId { get; set; }
        public virtual UserAssignments UserAssignment { get; set; }
    }
}