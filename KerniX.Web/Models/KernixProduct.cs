namespace KerniX.Web.Models
{
    public class KernixProduct
    {
        public int Id { get; set; }

        public string Name { get; set; }
            = string.Empty;

        public string Code { get; set; }
            = string.Empty;

        public string Category { get; set; }
            = string.Empty;

        public string Description { get; set; }
            = string.Empty;

        public decimal? Price { get; set; }

        public string PriceText { get; set; }
            = string.Empty;

        public string Url { get; set; }
            = string.Empty;

        public List<string> RecommendedFor { get; set; }
            = new();

        public List<string> Keywords { get; set; }
            = new();
    }
}