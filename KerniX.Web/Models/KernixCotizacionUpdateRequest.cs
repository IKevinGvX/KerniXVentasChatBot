namespace KerniX.Web.Models
{
    public class KernixCotizacionUpdateRequest
    {
        public string? NombreCliente { get; set; }

        public string? Empresa { get; set; }

        public string? Telefono { get; set; }

        public string? Email { get; set; }

        public decimal? Presupuesto { get; set; }

        public string? Uso { get; set; }

        public string? Necesidad { get; set; }

        public string? Observaciones { get; set; }
    }
}