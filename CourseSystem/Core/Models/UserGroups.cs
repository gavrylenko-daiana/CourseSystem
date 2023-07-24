using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class UserGroups : BaseEntity
    {
        public int AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; }
        public int GroupId { get; set; }
        public virtual Group Group { get; set; }

    }
}
