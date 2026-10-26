using KerniX.Web.Models;

namespace KerniX.Web.Services
{
    public class KernixActionService : IKernixActionService
    {
        // ============================================================
        // VENDEDORES KERNIX
        // ============================================================

        private const string Vendedor01 =
            "51930338447";

        private const string Vendedor02 =
            "51937213913";


        // ============================================================
        // DETECTAR ACCIONES
        // ============================================================

        public KernixActionResponse DetectActions(
            string message)
        {
            KernixActionResponse response =
                new();

            if (string.IsNullOrWhiteSpace(message))
            {
                response.Intent = "none";
                return response;
            }

            string text =
                message
                    .Trim()
                    .ToLowerInvariant();


            // ========================================================
            // PRIORIDAD 1
            // CONTACTAR VENDEDOR
            // ========================================================

            if (ContainsAny(
                text,
                "vendedor",
                "asesor",
                "whatsapp",
                "wsp",
                "contactar",
                "contacto",
                "hablar con alguien",
                "hablar con una persona",
                "hablar con vendedor",
                "hablar con un vendedor"))
            {
                response.Intent = "seller";

                string whatsappMessage =
                    """
                    Hola, vengo desde KerniX IA.

                    Me gustaría recibir atención de un asesor comercial de KerniX.

                    Quisiera obtener información sobre sus productos y/o solicitar una cotización.
                    """;

                response.Actions.Add(
                    new KernixAiAction
                    {
                        Type = "seller",

                        Label =
                            "Contactar por WhatsApp",

                        Url =
                            BuildWhatsAppUrl(
                                Vendedor01,
                                whatsappMessage
                            ),

                        External = true
                    }
                );

                return response;
            }


            // ========================================================
            // PRIORIDAD 2
            // COTIZACIÓN
            // ========================================================

            if (ContainsAny(
                text,
                "cotizar",
                "cotización",
                "cotizacion",
                "presupuesto",
                "precio para",
                "cuánto cuesta",
                "cuanto cuesta",
                "necesito comprar",
                "quiero comprar",
                "quiero cotizar",
                "solicitar cotización",
                "solicitar cotizacion"))
            {
                response.Intent = "quote";


                // ----------------------------------------------------
                // PRODUCTOS
                // ----------------------------------------------------

                response.Actions.Add(
                    new KernixAiAction
                    {
                        Type = "products",

                        Label =
                            "Ver productos",

                        Url =
                            "/Productos",

                        External =
                            false
                    }
                );


                // ----------------------------------------------------
                // WHATSAPP
                // ----------------------------------------------------

                string whatsappMessage =
                    """
                    Hola, vengo desde KerniX IA.

                    Estoy interesado en solicitar una cotización.

                    ¿Podrían ayudarme con información, disponibilidad y precio final?
                    """;

                response.Actions.Add(
                    new KernixAiAction
                    {
                        Type = "seller",

                        Label =
                            "Cotizar por WhatsApp",

                        Url =
                            BuildWhatsAppUrl(
                                Vendedor01,
                                whatsappMessage
                            ),

                        External =
                            true
                    }
                );

                return response;
            }


            // ========================================================
            // PRIORIDAD 3
            // PRODUCTOS
            // ========================================================

            if (ContainsAny(
                text,
                "producto",
                "productos",
                "laptop",
                "laptops",
                "computadora",
                "computadoras",
                "monitor",
                "monitores",
                "router",
                "routers",
                "switch",
                "switches",
                "teclado",
                "mouse",
                "accesorio",
                "accesorios",
                "equipamiento"))
            {
                response.Intent =
                    "products";


                // ----------------------------------------------------
                // CATÁLOGO
                // ----------------------------------------------------

                response.Actions.Add(
                    new KernixAiAction
                    {
                        Type =
                            "products",

                        Label =
                            "Ver productos",

                        Url =
                            "/Productos",

                        External =
                            false
                    }
                );


                // ----------------------------------------------------
                // CONSULTA WHATSAPP
                // ----------------------------------------------------

                string whatsappMessage =
                    """
                    Hola, vengo desde KerniX IA.

                    Estoy interesado en uno de sus productos.

                    Quisiera recibir más información y consultar disponibilidad.
                    """;

                response.Actions.Add(
                    new KernixAiAction
                    {
                        Type =
                            "seller",

                        Label =
                            "Consultar por WhatsApp",

                        Url =
                            BuildWhatsAppUrl(
                                Vendedor01,
                                whatsappMessage
                            ),

                        External =
                            true
                    }
                );

                return response;
            }


            // ========================================================
            // CONVERSACIÓN NORMAL
            // ========================================================

            response.Intent =
                "none";

            return response;
        }


        // ============================================================
        // CONSTRUIR URL WHATSAPP
        // ============================================================

        private static string BuildWhatsAppUrl(
            string phone,
            string message)
        {
            string encodedMessage =
                Uri.EscapeDataString(
                    message.Trim()
                );

            return
                $"https://wa.me/{phone}?text={encodedMessage}";
        }


        // ============================================================
        // BUSCAR PALABRAS
        // ============================================================

        private static bool ContainsAny(
            string text,
            params string[] values)
        {
            return values.Any(
                value =>
                    text.Contains(
                        value,
                        StringComparison.OrdinalIgnoreCase
                    )
            );
        }
    }
}