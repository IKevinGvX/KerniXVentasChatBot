namespace KerniX.Web.Models
{
    public class KernixCotizacionTemporalDetalle
    {
        public Guid Id { get; set; } =
            Guid.NewGuid();

        public int ProductId { get; set; }

        public string Codigo { get; set; } =
            string.Empty;

        public string Producto { get; set; } =
            string.Empty;

        public int Cantidad { get; set; } = 1;

        public decimal? PrecioReferencial { get; set; }

        public decimal? SubtotalReferencial
        {
            get
            {
                if (!PrecioReferencial.HasValue)
                {
                    return null;
                }

                return PrecioReferencial.Value *
                       Cantidad;
            }
        }
    }
}