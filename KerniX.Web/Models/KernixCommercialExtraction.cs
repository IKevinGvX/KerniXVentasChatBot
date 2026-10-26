namespace KerniX.Web.Models
{
    public class KernixCommercialExtraction
    {
        public string? NombreCliente { get; set; }

        public string? Telefono { get; set; }

        public decimal? Presupuesto { get; set; }

        public string? Uso { get; set; }

        public int? ProductId { get; set; }

        public int? Cantidad { get; set; }

        public bool TieneInformacionComercial =>
            !string.IsNullOrWhiteSpace(NombreCliente) ||
            !string.IsNullOrWhiteSpace(Telefono) ||
            Presupuesto.HasValue ||
            !string.IsNullOrWhiteSpace(Uso) ||
            ProductId.HasValue ||
            Cantidad.HasValue;
    }
}