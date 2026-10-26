namespace KerniX.Web.Models
{
    public class KernixCotizacionTemporal
    {
        public Guid Id { get; set; } =
            Guid.NewGuid();

        public Guid ConversationId { get; set; }

        public string NombreCliente { get; set; } =
            string.Empty;

        public string Empresa { get; set; } =
            string.Empty;

        public string Telefono { get; set; } =
            string.Empty;

        public string Email { get; set; } =
            string.Empty;

        public decimal? Presupuesto { get; set; }

        public string Uso { get; set; } =
            string.Empty;

        public string Necesidad { get; set; } =
            string.Empty;

        public string Observaciones { get; set; } =
            string.Empty;

        public string Estado { get; set; } =
            "Borrador";

        public bool ConfirmadoPorCliente { get; set; }

        public bool EnviadoAVendedor { get; set; }

        public string VendedorAsignado { get; set; } =
            string.Empty;

        public DateTime CreadoEn { get; set; } =
            DateTime.UtcNow;

        public DateTime ActualizadoEn { get; set; } =
            DateTime.UtcNow;

        public List<KernixCotizacionTemporalDetalle>
            Productos
        { get; set; } = new();
    }
}