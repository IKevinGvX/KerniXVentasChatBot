namespace KerniX.Web.Models
{
    public class KernixChatResponse
    {
        public string Message { get; set; } = string.Empty;

        public bool Success { get; set; } = true;

        public string Assistant { get; set; } = "KerniX IA";
    }
}