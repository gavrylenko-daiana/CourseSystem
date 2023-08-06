using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class ChatMessage : BaseEntity
    {
        public string AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; }

        public string Text { get; set; }

        public DateTime Created { get; set; }

        public int AssignmentId { get; set; }
        public virtual Assignment Assignment { get; set; }
    }
}
