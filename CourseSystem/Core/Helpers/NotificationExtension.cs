using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helpers
{
    public static class NotificationExtension
    {
        public static List<Notification> OrderByDate(this List<Notification> list)
        {
            return list.OrderByDescending(a => a.Created)
                        .ToList();
        }

        public static List<Notification> NotRead(this List<Notification> list)
        {
            return list.Where(n => !n.IsRead)
                        .ToList();
        }
    }
}
