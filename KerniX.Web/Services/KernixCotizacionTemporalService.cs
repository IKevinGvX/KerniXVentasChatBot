using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using KerniX.Web.Models;

namespace KerniX.Web.Services
{
    public class KernixCotizacionTemporalService
        : IKernixCotizacionTemporalService
    {
        private readonly ConcurrentDictionary<
            Guid,
            KernixCotizacionTemporal
        > _cotizaciones = new();

        private readonly IKernixProductCatalogService
            _productCatalog;

        public KernixCotizacionTemporalService(
            IKernixProductCatalogService productCatalog)
        {
            _productCatalog =
                productCatalog;
        }


        // ============================================================
        // OBTENER O CREAR
        // ============================================================

        public KernixCotizacionTemporal GetOrCreate(
            Guid conversationId)
        {
            return _cotizaciones.GetOrAdd(
                conversationId,
                id => new KernixCotizacionTemporal
                {
                    ConversationId = id,
                    Estado = "Borrador"
                }
            );
        }


        // ============================================================
        // OBTENER
        // ============================================================

        public KernixCotizacionTemporal? Get(
            Guid conversationId)
        {
            _cotizaciones.TryGetValue(
                conversationId,
                out KernixCotizacionTemporal? cotizacion
            );

            return cotizacion;
        }


        // ============================================================
        // ACTUALIZAR INFORMACIÓN DEL CLIENTE
        // ============================================================

        public KernixCotizacionTemporal Update(
            Guid conversationId,
            KernixCotizacionUpdateRequest request)
        {
            KernixCotizacionTemporal cotizacion =
                GetOrCreate(conversationId);

            lock (cotizacion)
            {
                if (!string.IsNullOrWhiteSpace(
                    request.NombreCliente))
                {
                    cotizacion.NombreCliente =
                        Clean(request.NombreCliente);
                }

                if (!string.IsNullOrWhiteSpace(
                    request.Empresa))
                {
                    cotizacion.Empresa =
                        Clean(request.Empresa);
                }

                if (!string.IsNullOrWhiteSpace(
                    request.Telefono))
                {
                    cotizacion.Telefono =
                        Clean(request.Telefono);
                }

                if (!string.IsNullOrWhiteSpace(
                    request.Email))
                {
                    cotizacion.Email =
                        Clean(request.Email);
                }

                if (request.Presupuesto.HasValue &&
                    request.Presupuesto.Value >= 0)
                {
                    cotizacion.Presupuesto =
                        request.Presupuesto.Value;
                }

                if (!string.IsNullOrWhiteSpace(
                    request.Uso))
                {
                    cotizacion.Uso =
                        Clean(request.Uso);
                }

                if (!string.IsNullOrWhiteSpace(
                    request.Necesidad))
                {
                    cotizacion.Necesidad =
                        Clean(request.Necesidad);
                }

                if (!string.IsNullOrWhiteSpace(
                    request.Observaciones))
                {
                    cotizacion.Observaciones =
                        Clean(request.Observaciones);
                }

                cotizacion.ActualizadoEn =
                    DateTime.UtcNow;

                UpdateState(cotizacion);
            }

            return cotizacion;
        }


        // ============================================================
        // AGREGAR PRODUCTO
        // ============================================================

        public KernixCotizacionTemporal AddProduct(
            Guid conversationId,
            int productId,
            int cantidad)
        {
            if (cantidad <= 0)
            {
                throw new ArgumentException(
                    "La cantidad debe ser mayor a 0."
                );
            }

            if (cantidad > 999)
            {
                throw new ArgumentException(
                    "La cantidad solicitada es demasiado alta."
                );
            }

            KernixProduct? product =
                _productCatalog.GetProduct(
                    productId
                );

            if (product == null)
            {
                throw new ArgumentException(
                    "El producto indicado no existe en el catálogo KerniX."
                );
            }

            KernixCotizacionTemporal cotizacion =
                GetOrCreate(conversationId);

            lock (cotizacion)
            {
                KernixCotizacionTemporalDetalle? existing =
                    cotizacion.Productos
                        .FirstOrDefault(
                            p => p.ProductId == productId
                        );

                if (existing != null)
                {
                    existing.Cantidad =
                        cantidad;

                    existing.PrecioReferencial =
                        product.Price;
                }
                else
                {
                    cotizacion.Productos.Add(
                        new KernixCotizacionTemporalDetalle
                        {
                            ProductId =
                                product.Id,

                            Codigo =
                                product.Code,

                            Producto =
                                product.Name,

                            Cantidad =
                                cantidad,

                            PrecioReferencial =
                                product.Price
                        }
                    );
                }

                cotizacion.ActualizadoEn =
                    DateTime.UtcNow;

                UpdateState(cotizacion);
            }

            return cotizacion;
        }


        // ============================================================
        // ELIMINAR PRODUCTO
        // ============================================================

        public KernixCotizacionTemporal RemoveProduct(
            Guid conversationId,
            int productId)
        {
            KernixCotizacionTemporal cotizacion =
                GetOrCreate(conversationId);

            lock (cotizacion)
            {
                cotizacion.Productos.RemoveAll(
                    product =>
                        product.ProductId == productId
                );

                cotizacion.ActualizadoEn =
                    DateTime.UtcNow;

                UpdateState(cotizacion);
            }

            return cotizacion;
        }


        // ============================================================
        // CAMPOS FALTANTES
        // ============================================================

        public IReadOnlyList<string> GetMissingFields(
      Guid conversationId)
        {
            KernixCotizacionTemporal? cotizacion =
                Get(conversationId);

            List<string> missing =
                new();


            // ============================================================
            // NO EXISTE COTIZACIÓN
            // ============================================================

            if (cotizacion == null)
            {
                missing.Add("nombre");
                missing.Add("teléfono o correo");
                missing.Add("producto");
                missing.Add("cantidad");
                missing.Add("uso o necesidad");

                return missing;
            }


            // ============================================================
            // VALIDAR COTIZACIÓN
            // ============================================================

            lock (cotizacion)
            {
                // --------------------------------------------------------
                // NOMBRE
                // --------------------------------------------------------

                if (string.IsNullOrWhiteSpace(
                    cotizacion.NombreCliente))
                {
                    missing.Add("nombre");
                }


                // --------------------------------------------------------
                // MEDIO DE CONTACTO
                // --------------------------------------------------------

                bool hasPhone =
                    !string.IsNullOrWhiteSpace(
                        cotizacion.Telefono
                    );

                bool hasEmail =
                    !string.IsNullOrWhiteSpace(
                        cotizacion.Email
                    );

                if (!hasPhone && !hasEmail)
                {
                    missing.Add(
                        "teléfono o correo"
                    );
                }


                // --------------------------------------------------------
                // PRODUCTOS
                // --------------------------------------------------------

                if (
                    cotizacion.Productos == null ||
                    cotizacion.Productos.Count == 0)
                {
                    missing.Add(
                        "producto"
                    );
                }
                else
                {
                    // ----------------------------------------------------
                    // CANTIDADES
                    // ----------------------------------------------------

                    bool invalidQuantity =
                        cotizacion.Productos.Any(
                            producto =>
                                producto.Cantidad <= 0
                        );

                    if (invalidQuantity)
                    {
                        missing.Add(
                            "cantidad"
                        );
                    }
                }


                // --------------------------------------------------------
                // USO / NECESIDAD
                // --------------------------------------------------------

                bool hasUse =
                    !string.IsNullOrWhiteSpace(
                        cotizacion.Uso
                    );

                bool hasNeed =
                    !string.IsNullOrWhiteSpace(
                        cotizacion.Necesidad
                    );

                if (!hasUse && !hasNeed)
                {
                    missing.Add(
                        "uso o necesidad"
                    );
                }


                // --------------------------------------------------------
                // PRESUPUESTO
                //
                // NO lo hacemos obligatorio.
                // Un cliente puede solicitar cotización sin presupuesto.
                // --------------------------------------------------------

                if (
                    cotizacion.Presupuesto.HasValue &&
                    cotizacion.Presupuesto.Value <= 0)
                {
                    missing.Add(
                        "presupuesto válido"
                    );
                }
            }


            return missing;
        }

        // ============================================================
        // ESTÁ LISTA
        // ============================================================

        public bool IsReady(
            Guid conversationId)
        {
            return GetMissingFields(
                conversationId
            ).Count == 0;
        }


        // ============================================================
        // CONFIRMAR
        // ============================================================

        public KernixCotizacionTemporal Confirm(
            Guid conversationId)
        {
            KernixCotizacionTemporal cotizacion =
                GetOrCreate(conversationId);

            IReadOnlyList<string> missing =
                GetMissingFields(
                    conversationId
                );

            if (missing.Count > 0)
            {
                throw new InvalidOperationException(
                    "No se puede confirmar la solicitud. " +
                    "Falta: " +
                    string.Join(", ", missing) +
                    "."
                );
            }

            lock (cotizacion)
            {
                cotizacion.ConfirmadoPorCliente =
                    true;

                cotizacion.Estado =
                    "Confirmado";

                cotizacion.ActualizadoEn =
                    DateTime.UtcNow;
            }

            return cotizacion;
        }


        // ============================================================
        // MENSAJE PARA EL VENDEDOR
        // ============================================================

        public string BuildSellerMessage(
            Guid conversationId)
        {
            KernixCotizacionTemporal? cotizacion =
                Get(conversationId);

            if (cotizacion == null)
            {
                throw new InvalidOperationException(
                    "No existe una solicitud temporal."
                );
            }

            if (!cotizacion.ConfirmadoPorCliente)
            {
                throw new InvalidOperationException(
                    "El cliente todavía no ha confirmado el envío."
                );
            }

            StringBuilder message =
                new();

            message.AppendLine(
                "SOLICITUD DE COTIZACIÓN - KERNIX IA"
            );

            message.AppendLine();

            message.AppendLine(
                $"Cliente: {ValueOrPending(cotizacion.NombreCliente)}"
            );

            message.AppendLine(
                $"Empresa: {ValueOrPending(cotizacion.Empresa)}"
            );

            message.AppendLine(
                $"Teléfono: {ValueOrPending(cotizacion.Telefono)}"
            );

            message.AppendLine(
                $"Correo: {ValueOrPending(cotizacion.Email)}"
            );

            message.AppendLine(
                $"Uso: {ValueOrPending(cotizacion.Uso)}"
            );

            message.AppendLine(
                $"Necesidad: {ValueOrPending(cotizacion.Necesidad)}"
            );

            if (cotizacion.Presupuesto.HasValue)
            {
                message.AppendLine(
                    $"Presupuesto: {Money(cotizacion.Presupuesto.Value)}"
                );
            }

            message.AppendLine();

            message.AppendLine("PRODUCTOS:");

            decimal totalKnown =
                0;

            bool hasKnownPrices =
                false;

            foreach (
                KernixCotizacionTemporalDetalle product
                in cotizacion.Productos)
            {
                message.AppendLine();

                message.AppendLine(
                    $"• {product.Producto}"
                );

                message.AppendLine(
                    $"  Código: {product.Codigo}"
                );

                message.AppendLine(
                    $"  Cantidad: {product.Cantidad}"
                );

                if (product.PrecioReferencial.HasValue)
                {
                    hasKnownPrices = true;

                    decimal subtotal =
                        product.SubtotalReferencial
                        ?? 0;

                    totalKnown +=
                        subtotal;

                    message.AppendLine(
                        $"  Precio referencial: {Money(product.PrecioReferencial.Value)}"
                    );

                    message.AppendLine(
                        $"  Subtotal referencial: {Money(subtotal)}"
                    );
                }
                else
                {
                    message.AppendLine(
                        "  Precio: Consultar"
                    );
                }
            }

            if (hasKnownPrices)
            {
                message.AppendLine();

                message.AppendLine(
                    $"TOTAL REFERENCIAL CON PRECIOS CONOCIDOS: {Money(totalKnown)}"
                );
            }

            if (!string.IsNullOrWhiteSpace(
                cotizacion.Observaciones))
            {
                message.AppendLine();

                message.AppendLine(
                    $"Observaciones: {cotizacion.Observaciones}"
                );
            }

            message.AppendLine();

            message.AppendLine(
                "La disponibilidad, precio final y condiciones comerciales deben ser confirmadas por el vendedor."
            );

            message.AppendLine();

            message.AppendLine(
                $"ConversationId: {cotizacion.ConversationId}"
            );

            return message
                .ToString()
                .Trim();
        }


        // ============================================================
        // WHATSAPP
        // ============================================================

        public string BuildWhatsAppUrl(
      Guid conversationId,
      string vendedor)
        {
            KernixCotizacionTemporal cotizacion =
                Get(conversationId)
                ?? throw new InvalidOperationException(
                    "No existe una solicitud temporal."
                );


            // ============================================================
            // VALIDAR QUE TENGAMOS INFORMACIÓN SUFICIENTE
            // ============================================================

            IReadOnlyList<string> missing =
                GetMissingFields(conversationId);

            if (missing.Count > 0)
            {
                throw new InvalidOperationException(
                    "La solicitud todavía está incompleta. Falta: " +
                    string.Join(", ", missing) +
                    "."
                );
            }


            // ============================================================
            // SELECCIONAR VENDEDOR
            // ============================================================

            string phone;
            string sellerName;

            if (
                vendedor.Equals(
                    "2",
                    StringComparison.OrdinalIgnoreCase
                ))
            {
                phone =
                    "51937213913";

                sellerName =
                    "Vendedor 02";
            }
            else
            {
                phone =
                    "51930338447";

                sellerName =
                    "Vendedor 01";
            }


            // ============================================================
            // PREPARAR MENSAJE
            // ============================================================

            StringBuilder message =
                new();

            message.AppendLine(
                "Hola, vengo desde KerniX IA."
            );

            message.AppendLine();

            message.AppendLine(
                "SOLICITUD DE COTIZACIÓN"
            );

            message.AppendLine();

            message.AppendLine(
                $"Cliente: {ValueOrPending(cotizacion.NombreCliente)}"
            );

            if (!string.IsNullOrWhiteSpace(
                cotizacion.Empresa))
            {
                message.AppendLine(
                    $"Empresa: {cotizacion.Empresa}"
                );
            }

            if (!string.IsNullOrWhiteSpace(
                cotizacion.Telefono))
            {
                message.AppendLine(
                    $"Teléfono: {cotizacion.Telefono}"
                );
            }

            if (!string.IsNullOrWhiteSpace(
                cotizacion.Email))
            {
                message.AppendLine(
                    $"Correo: {cotizacion.Email}"
                );
            }

            if (!string.IsNullOrWhiteSpace(
                cotizacion.Uso))
            {
                message.AppendLine(
                    $"Uso: {cotizacion.Uso}"
                );
            }

            if (!string.IsNullOrWhiteSpace(
                cotizacion.Necesidad))
            {
                message.AppendLine(
                    $"Necesidad: {cotizacion.Necesidad}"
                );
            }


            // ============================================================
            // PRESUPUESTO
            // ============================================================

            if (cotizacion.Presupuesto.HasValue)
            {
                message.AppendLine(
                    $"Presupuesto: {Money(cotizacion.Presupuesto.Value)}"
                );
            }


            // ============================================================
            // PRODUCTOS
            // ============================================================

            message.AppendLine();
            message.AppendLine("Productos:");

            decimal total =
                0;

            bool hasKnownPrices =
                false;


            foreach (
                KernixCotizacionTemporalDetalle product
                in cotizacion.Productos)
            {
                message.AppendLine(
                    $"- {product.Cantidad} x {product.Producto} ({product.Codigo})"
                );

                if (product.PrecioReferencial.HasValue)
                {
                    decimal subtotal =
                        product.PrecioReferencial.Value *
                        product.Cantidad;

                    total +=
                        subtotal;

                    hasKnownPrices =
                        true;

                    message.AppendLine(
                        $"  Precio referencial: {Money(product.PrecioReferencial.Value)}"
                    );

                    message.AppendLine(
                        $"  Subtotal: {Money(subtotal)}"
                    );
                }
                else
                {
                    message.AppendLine(
                        "  Precio: Consultar"
                    );
                }
            }


            // ============================================================
            // TOTAL
            // ============================================================

            if (hasKnownPrices)
            {
                message.AppendLine();

                message.AppendLine(
                    $"Total referencial: {Money(total)}"
                );
            }


            // ============================================================
            // MENSAJE FINAL
            // ============================================================

            message.AppendLine();

            message.AppendLine(
                "Quisiera continuar con la cotización y confirmar disponibilidad, precio final y condiciones comerciales."
            );


            // ============================================================
            // GUARDAR SOLO QUE FUE PREPARADO
            // ============================================================

            lock (cotizacion)
            {
                cotizacion.VendedorAsignado =
                    sellerName;

                // IMPORTANTE:
                // generar el enlace NO significa que WhatsApp fue enviado.

                cotizacion.EnviadoAVendedor =
                    false;

                cotizacion.Estado =
                    "Preparado para WhatsApp";

                cotizacion.ActualizadoEn =
                    DateTime.UtcNow;
            }


            // ============================================================
            // GENERAR URL
            // ============================================================

            string encoded =
                Uri.EscapeDataString(
                    message
                        .ToString()
                        .Trim()
                );


            return
                $"https://wa.me/{phone}?text={encoded}";
        }

        // ============================================================
        // ELIMINAR
        // ============================================================

        public bool Delete(
            Guid conversationId)
        {
            return _cotizaciones.TryRemove(
                conversationId,
                out _
            );
        }


        // ============================================================
        // ESTADO
        // ============================================================

        private static void UpdateState(
            KernixCotizacionTemporal cotizacion)
        {
            if (cotizacion.EnviadoAVendedor)
            {
                cotizacion.Estado =
                    "Enviado";

                return;
            }

            if (cotizacion.ConfirmadoPorCliente)
            {
                cotizacion.Estado =
                    "Confirmado";

                return;
            }

            bool hasClient =
                !string.IsNullOrWhiteSpace(
                    cotizacion.NombreCliente
                );

            bool hasContact =
                !string.IsNullOrWhiteSpace(
                    cotizacion.Telefono
                )
                ||
                !string.IsNullOrWhiteSpace(
                    cotizacion.Email
                );

            bool hasProducts =
                cotizacion.Productos.Count > 0;

            bool hasUse =
                !string.IsNullOrWhiteSpace(
                    cotizacion.Uso
                );

            cotizacion.Estado =
                hasClient &&
                hasContact &&
                hasProducts &&
                hasUse
                    ? "Completo"
                    : "Borrador";
        }


        // ============================================================
        // HELPERS
        // ============================================================

        private static string Clean(
            string value)
        {
            return value
                .Replace("\0", string.Empty)
                .Trim();
        }

        private static string ValueOrPending(
            string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "No indicado"
                : value.Trim();
        }

        private static string Money(
            decimal value)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "S/ {0:N2}",
                value
            );
        }
    }
}