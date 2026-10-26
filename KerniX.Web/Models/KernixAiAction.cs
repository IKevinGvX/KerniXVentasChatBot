namespace KerniX.Web.Models
{
    public class KernixAiAction
    {
        public string Type { get; set; } = "none";

        public string Label { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public bool External { get; set; }
    }
}