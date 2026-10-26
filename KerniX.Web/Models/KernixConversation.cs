namespace KerniX.Web.Models
{
    public class KernixConversation
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title { get; set; } = "Nueva conversación";

        public string Summary { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<KernixConversationMessage> Messages { get; set; }
            = new();
    }
}