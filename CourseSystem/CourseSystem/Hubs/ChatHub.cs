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

        public ChatHub(IChatMessageService chatMessageService, UserManager<AppUser> userManager,
            IAssignmentService assignmentService)
        {
            _chatMessageService = chatMessageService;
            _userManager = userManager;
            _assignmentService = assignmentService;
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

            var user = await _userManager.GetUserAsync(Context.User);

            if (user == null)
            {
                await Clients.Group(assignmentId).SendAsync("Error", "Unknown user");
            }

            var assignmentResult = await _assignmentService.GetById(int.Parse(assignmentId));

            if (!assignmentResult.IsSuccessful)
            {
                await Clients.Group(assignmentId).SendAsync("Error", "Unknown assignment");
            }

            var message = await _chatMessageService.CreateChatMessage(user, assignmentResult.Data, text);

            if (!message.IsSuccessful)
            {
                await Clients.Group(assignmentId).SendAsync("Error", message.Message);
            }
            else
            {
                await Clients.Group(message.Data.AssignmentId.ToString()).SendAsync("Receive", message.Data.Text,
                    $"{user.FirstName} {user.LastName}", message.Data.Created.ToString("g"));
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
            catch (Exception ex)
            {
                throw new Exception($"User was not added to chat! Exception:{ex.Message}");
            }
        }
    }
}