namespace KerniX.Web.Models
{
    public class KernixChatRequest
    {
        public string Message { get; set; } = string.Empty;

        public List<KernixHistoryMessage> History { get; set; } = new();
    }

    public class KernixHistoryMessage
    {
        public string Role { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
    }
}