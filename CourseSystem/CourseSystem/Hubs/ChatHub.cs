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
        private readonly ILogger<ChatHub> _logger;
        
        public ChatHub(IChatMessageService chatMessageService, IAssignmentService assignmentService, IUserService userService, ILogger<ChatHub> logger)
        {
            _chatMessageService = chatMessageService;
            _assignmentService = assignmentService;
            _userService = userService;
            _logger = logger;
        }

        public async Task Send(string text, string assignmentId)
        {

            if (string.IsNullOrEmpty(assignmentId))
            {
                _logger.LogError("Chat failure: assignmentId wasn't received");

                return;
            }

            var currentUserResult = await _userService.GetCurrentUser(Context.User);

            if (!currentUserResult.IsSuccessful)
            {
                _logger.LogWarning("Unauthorized user");
                await Clients.Group(assignmentId).SendAsync("Error", "Unknown user! Try to refresh this page.");

                return;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogInformation("User {userId} tried to send empty message or white spaces", currentUserResult.Data.Id);
                await Clients.Group(assignmentId).SendAsync("Error", "Empty message");

                return;
            }

            var assignmentResult = await _assignmentService.GetById(int.Parse(assignmentId));

            if (!assignmentResult.IsSuccessful)
            {
                _logger.LogError("Failed to get assignment by Id {assignmentId}! Error: {errorMessage}", assignmentId, assignmentResult.Message);
                await Clients.Group(assignmentId).SendAsync("Error", "Unknown assignment! Try to reload this page.");

                return;
            }

            var messageResult = await _chatMessageService.CreateChatMessage(currentUserResult.Data, assignmentResult.Data, text);

            if (!messageResult.IsSuccessful)
            {
                _logger.LogError("Failed to create chat message for assignment {assignmentId}! Error: {errorMessage}", assignmentId, messageResult.Message);

                await Clients.Group(assignmentId).SendAsync("Error", "Something went wrong! Please, try again.");
            }
            else
            {
                await Clients.Group(messageResult.Data.AssignmentId.ToString()).SendAsync("Receive", messageResult.Data.Text, $"{currentUserResult.Data.FirstName} {currentUserResult.Data.LastName}", messageResult.Data.Created.ToString("g"));
            }
        }

        public async Task AddToAssignmentChat(string assignmentId)
        {
            if (string.IsNullOrEmpty(assignmentId))
            {
                _logger.LogError("Chat failure: assignmentId wasn't received");
                throw new ArgumentNullException(nameof(assignmentId));
            }
            else
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, assignmentId);
            }
        }
    }
}