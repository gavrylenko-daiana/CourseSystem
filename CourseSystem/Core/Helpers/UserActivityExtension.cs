using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helpers
{
    public static class UserActivityExtension
    {
        public static List<UserActivity> ForMonth(this List<UserActivity> activities, DateTime month)
        {
            return activities
                .Where(a => a.Created.Month == month.Month)
                    .OrderByDescending(a => a.Created)
                        .ToList();
        }

        public static List<UserActivity> ForDate(this List<UserActivity> activities, DateTime date)
        {
            return activities
                .Where(a => a.Created.Date == date.Date)
                    .OrderByDescending(a => a.Created)
                        .ToList();
        }
    }
}
