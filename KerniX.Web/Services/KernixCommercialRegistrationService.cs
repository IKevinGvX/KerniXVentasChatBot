using KerniX.Web.Models;

namespace KerniX.Web.Services
{
    public class KernixCommercialRegistrationService
    {
        private readonly IKernixCommercialExtractorService
            _extractor;

        private readonly IKernixCotizacionTemporalService
            _cotizacionService;

        private readonly ILogger<KernixCommercialRegistrationService>
            _logger;

        public KernixCommercialRegistrationService(
            IKernixCommercialExtractorService extractor,
            IKernixCotizacionTemporalService cotizacionService,
            ILogger<KernixCommercialRegistrationService> logger)
        {
            _extractor =
                extractor;

            _cotizacionService =
                cotizacionService;

            _logger =
                logger;
        }


        public KernixCotizacionTemporal? ProcessMessage(
            Guid conversationId,
            string message)
        {
            KernixCommercialExtraction extraction =
                _extractor.Extract(
                    message
                );

            if (!extraction.TieneInformacionComercial)
            {
                return _cotizacionService.Get(
                    conversationId
                );
            }


            // ========================================================
            // DATOS GENERALES
            // ========================================================

            KernixCotizacionUpdateRequest update =
                new()
                {
                    NombreCliente =
                        extraction.NombreCliente,

                    Telefono =
                        extraction.Telefono,

                    Presupuesto =
                        extraction.Presupuesto,

                    Uso =
                        extraction.Uso
                };

            KernixCotizacionTemporal cotizacion =
                _cotizacionService.Update(
                    conversationId,
                    update
                );


            // ========================================================
            // PRODUCTO
            // ========================================================

            if (extraction.ProductId.HasValue)
            {
                int quantity =
                    extraction.Cantidad
                    ?? 1;

                cotizacion =
                    _cotizacionService.AddProduct(
                        conversationId,
                        extraction.ProductId.Value,
                        quantity
                    );
            }


            _logger.LogInformation(
                "Registro comercial actualizado. ConversationId: {ConversationId}, Estado: {Estado}",
                conversationId,
                cotizacion.Estado
            );

            return cotizacion;
        }
    }
}