﻿using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helpers
{
    public static class NotificationExtension
    {
        public static Result<List<Notification>> OrderByDate(this List<Notification> list)
        {
            if (list == null)
            {
                return new Result<List<Notification>>(false, nameof(list));
            }

            var notification = list.OrderByDescending(a => a.Created).ToList();

            return new Result<List<Notification>>(true, notification);
        }

        public static Result<List<Notification>> NotReadByDate(this List<Notification> list)
        {
            if (list == null)
            {
                return new Result<List<Notification>>(false, nameof(list));
            }

            var notification = list.Where(n => !n.IsRead)
                .OrderByDescending(a => a.Created)
                .ToList();

            return new Result<List<Notification>>(true, notification);
        }
    }
}