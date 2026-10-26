namespace KerniX.Web.Models
{
    public class KernixActionResponse
    {
        public bool Success { get; set; } = true;

        public string Intent { get; set; } = "none";

        public List<KernixAiAction> Actions { get; set; }
            = new();
    }
}