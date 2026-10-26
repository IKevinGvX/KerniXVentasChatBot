using KerniX.Web.Models;

namespace KerniX.Web.Services
{
    public interface IKernixCotizacionTemporalService
    {
        KernixCotizacionTemporal GetOrCreate(
            Guid conversationId
        );

        KernixCotizacionTemporal? Get(
            Guid conversationId
        );

        KernixCotizacionTemporal Update(
            Guid conversationId,
            KernixCotizacionUpdateRequest request
        );

        KernixCotizacionTemporal AddProduct(
            Guid conversationId,
            int productId,
            int cantidad
        );

        KernixCotizacionTemporal RemoveProduct(
            Guid conversationId,
            int productId
        );

        bool IsReady(
            Guid conversationId
        );

        IReadOnlyList<string> GetMissingFields(
            Guid conversationId
        );

        KernixCotizacionTemporal Confirm(
            Guid conversationId
        );

        string BuildSellerMessage(
            Guid conversationId
        );

        string BuildWhatsAppUrl(
            Guid conversationId,
            string vendedor
        );

        bool Delete(
            Guid conversationId
        );
    }
}