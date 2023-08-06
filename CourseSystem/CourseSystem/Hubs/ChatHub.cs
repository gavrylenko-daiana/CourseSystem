using BLL.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace UI.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatMessageService _chatMessageService;
        private readonly IAssignmentService _assignmentService;
        private readonly UserManager<AppUser> _userManager;
        public ChatHub(IChatMessageService chatMessageService, UserManager<AppUser> userManager, IAssignmentService assignmentService)
        {
            _chatMessageService = chatMessageService;
            _userManager = userManager;
            _assignmentService = assignmentService;
        }
        public async Task Send(string text, string assignmentId)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(text))
                {
                    return;
                }

                if(assignmentId == null)
                {
                    throw new ArgumentNullException(nameof(assignmentId));
                }

                var user = await _userManager.GetUserAsync(Context.User);
                var assignment = await _assignmentService.GetById(int.Parse(assignmentId));

                var message = await _chatMessageService.CreateChatMessage(user, assignment, text);
                await Clients.Group(message.AssignmentId.ToString()).SendAsync("Receive", message.Text, $"{user.FirstName} {user.LastName}", message.Created.ToString("g"));
            }
            catch (Exception ex)
            {
                throw new Exception($"Chat failure! Exception: {ex.Message}");
            }
        }

        public async Task AddToAssignmentChat(string assignmentId)
        {
            try
            {
                if (assignmentId == null)
                {
                    throw new ArgumentNullException(nameof(assignmentId));
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, assignmentId);
            }
            catch(Exception ex)
            {
                throw new Exception($"User wasn`t added to chat! Exception:{ex.Message}");
            }
        }
    }
}
