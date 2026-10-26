namespace KerniX.Web.Models
{
    public class KernixConversationMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ConversationId { get; set; }

        public string Role { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}