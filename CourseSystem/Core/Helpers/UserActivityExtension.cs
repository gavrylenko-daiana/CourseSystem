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
        public static Result<List<UserActivity>> ForMonth(this List<UserActivity> activities, DateTime month)
        {
            if (activities == null)
            {
                return new Result<List<UserActivity>>(false, nameof(activities));
            }

            if (month == DateTime.MinValue)
            {
                return new Result<List<UserActivity>>(false, nameof(month));
            }

            return new Result<List<UserActivity>>(true,
                activities.Where(a => a.Created.Month == month.Month)
                    .OrderByDescending(a => a.Created)
                        .ToList());
        }

        public static Result<List<UserActivity>> ForDate(this List<UserActivity> activities, DateTime date)
        {
            if (activities == null)
            {
                return new Result<List<UserActivity>>(false, nameof(activities));
            }

            if (date == DateTime.MinValue)
            {
                return new Result<List<UserActivity>>(false, nameof(date));
            }

            return new Result<List<UserActivity>>(true,
                activities.Where(a => a.Created.Date == date.Date)
                    .OrderByDescending(a => a.Created)
                        .ToList());
        }
    }
}
