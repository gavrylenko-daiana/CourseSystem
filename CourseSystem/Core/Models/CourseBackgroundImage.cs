using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class CourseBackgroundImage : BaseEntity
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
    }
}
