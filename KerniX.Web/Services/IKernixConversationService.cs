using KerniX.Web.Models;

namespace KerniX.Web.Services
{
    public interface IKernixConversationService
    {
        KernixConversation CreateConversation();

        KernixConversation? GetConversation(
            Guid conversationId
        );

        IReadOnlyCollection<KernixConversation>
            GetConversations();

        KernixConversationMessage AddMessage(
            Guid conversationId,
            string role,
            string content
        );

        IReadOnlyList<KernixConversationMessage>
            GetMessages(
                Guid conversationId
            );

        void UpdateSummary(
            Guid conversationId,
            string summary
        );

        bool DeleteConversation(
            Guid conversationId
        );

        void Clear();
    }
}