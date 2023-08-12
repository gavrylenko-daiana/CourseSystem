using Core.Models;

namespace UI.ViewModels
{
    public class AssignmentChatViewModel
    {
        public int Id { get; set; }
        public List<ChatMessage> ChatMessages { get; set; }
    }
}
