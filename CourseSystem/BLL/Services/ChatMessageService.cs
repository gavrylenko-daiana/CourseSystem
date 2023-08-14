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

        public async Task<Result<ChatMessage>> CreateChatMessage(AppUser appUser, Assignment assignment, string text)
        {
            if (appUser == null)
            {
                return new Result<ChatMessage>(false, "Invalid user");
            }

            if (assignment == null)
            {
                return new Result<ChatMessage>(false, "Invalid assignment");
            }

            if (String.IsNullOrWhiteSpace(text))
            {
                return new Result<ChatMessage>(false, "Invalid text");
            }

            var message = new ChatMessage()
            {
                AppUser = appUser,
                Assignment = assignment,
                Text = text,
                Created = DateTime.Now,
            };

            try
            {
                await _repository.AddAsync(message);
                await _unitOfWork.Save();

                return new Result<ChatMessage>(true, message);
            }
            catch (Exception ex)
            {
                return new Result<ChatMessage>(false, ex.Message);
            }
        }
    }
}