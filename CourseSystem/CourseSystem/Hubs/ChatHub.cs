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
        private readonly IUserService _userService;
        public ChatHub(IChatMessageService chatMessageService, IAssignmentService assignmentService, IUserService userService)
        {
            _chatMessageService = chatMessageService;
            _assignmentService = assignmentService;
            _userService = userService;
        }
        public async Task Send(string text, string assignmentId)
        {
            if (String.IsNullOrWhiteSpace(text))
            {
                await Clients.Group(assignmentId).SendAsync("Error", "Empty message");
            }

            if (assignmentId == null)
            {
                return;
            }

            var userResult = await _userService.GetCurrentUser(Context.User);

            if (!userResult.IsSuccessful)
            {
                await Clients.Group(assignmentId).SendAsync("Error", "Unknown user");
            }

            var assignmentResult = await _assignmentService.GetById(int.Parse(assignmentId));

            if (!assignmentResult.IsSuccessful)
            {
                await Clients.Group(assignmentId).SendAsync("Error", "Unknown assignment");
            }

            var message = await _chatMessageService.CreateChatMessage(userResult.Data, assignmentResult.Data, text);

            if (!message.IsSuccessful)
            {
                await Clients.Group(assignmentId).SendAsync("Error", message.Message);
            }
            else
            {
                await Clients.Group(message.Data.AssignmentId.ToString()).SendAsync("Receive", message.Data.Text, $"{userResult.Data.FirstName} {userResult.Data.LastName}", message.Data.Created.ToString("g"));
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
                throw new Exception($"User was not added to chat! Exception:{ex.Message}");
            }
        }
    }
}
