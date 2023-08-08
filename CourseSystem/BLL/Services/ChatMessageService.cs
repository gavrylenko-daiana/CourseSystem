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
    public class ChatMessageService : GenericService<ChatMessage>, IChatMessageService
    {
        public ChatMessageService(UnitOfWork unitOfWork) 
            : base(unitOfWork, unitOfWork.ChatMessageRepository)
        {
        }
        public async Task<ChatMessage> CreateChatMessage(AppUser appUser, Assignment assignment, string text)
        {
            try
            {
                if (appUser == null)
                {
                    throw new ArgumentNullException(nameof(appUser));
                }

                if (assignment == null)
                {
                    throw new ArgumentNullException(nameof(assignment));
                }

                if (String.IsNullOrWhiteSpace(text))
                {
                    throw new ArgumentNullException(nameof(text));
                }

                var message = new ChatMessage()
                {
                    AppUser = appUser,
                    Assignment = assignment,
                    Text = text,
                    Created = DateTime.Now,
                };

                await _repository.AddAsync(message);
                await _unitOfWork.Save();

                return message;

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create chat message. Exception: {ex.Message}");
            }
        }
    }
}
