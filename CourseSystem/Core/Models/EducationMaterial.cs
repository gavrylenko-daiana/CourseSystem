using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class EducationMaterial : BaseEntity
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string FileExtension { get; set; }
        public DateTime UploadTime { get; set; }
        public MaterialAccess MaterialAccess { get; set; }
        public int? CourseId { get; set; }
        public virtual Course? Course { get; set; }
        public int? GroupId { get; set; }
        public virtual Group? Group { get; set; }
    }
}