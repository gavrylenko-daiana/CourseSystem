using BLL.Interfaces;
using Core.Models;
using DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class NotificationService : GenericService<Notification>, INotificationService
    {
        public NotificationService(UnitOfWork unitOfWork) : base(unitOfWork)
        {
            _repository = unitOfWork.NotificationRepository;
        }

        public async Task MarkAsRead(Notification notification)
        {
            try
            {
                if (notification == null)
                {
                    throw new Exception("Notification was null");
                }

                notification.IsRead = true;

                await Update(notification);
                await _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to mark notification as read. Exception: {ex.Message}");
            }
        }
    }
}
