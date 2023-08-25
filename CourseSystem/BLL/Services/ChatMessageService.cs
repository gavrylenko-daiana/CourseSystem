using BLL.Interfaces;
using Core.Models;
using DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BLL.Services
{
    public class ChatMessageService : GenericService<ChatMessage>, IChatMessageService
    {
        private readonly ILogger<AssignmentService> _logger;
        
        public ChatMessageService(UnitOfWork unitOfWork, ILogger<AssignmentService> logger)
            : base(unitOfWork, unitOfWork.ChatMessageRepository)
        {
            _logger = logger;
        }

        public async Task<Result<ChatMessage>> CreateChatMessage(AppUser appUser, Assignment assignment, string text)
        {
            if (appUser == null)
            {
                _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(appUser));
                
                return new Result<ChatMessage>(false, "Invalid user");
            }

            if (assignment == null)
            {
                _logger.LogError("Failed to {action}, {entity} was null!", MethodBase.GetCurrentMethod()?.Name, nameof(assignment));
                
                return new Result<ChatMessage>(false, "Invalid assignment");
            }

            if (String.IsNullOrWhiteSpace(text))
            {
                _logger.LogError("Failed to {action}, text for chat is null or white space", MethodBase.GetCurrentMethod()?.Name);
                
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

                _logger.LogInformation("Successfully {action} with {text} in {assignmentName} chat",
                    MethodBase.GetCurrentMethod()?.Name, text, assignment.Name);
                
                return new Result<ChatMessage>(true, message);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Failed to {action} with {text} in {assignmentName} chat. Error: {errorMsg}",
                    MethodBase.GetCurrentMethod()?.Name, text, assignment.Name, ex.Message);
                
                return new Result<ChatMessage>(false, ex.Message);
            }
        }
    }
}