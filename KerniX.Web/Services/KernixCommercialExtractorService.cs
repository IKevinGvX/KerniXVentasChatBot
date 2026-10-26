using System.Globalization;
using System.Text.RegularExpressions;
using KerniX.Web.Models;

namespace KerniX.Web.Services
{
    public class KernixCommercialExtractorService
        : IKernixCommercialExtractorService
    {
        public KernixCommercialExtraction Extract(
            string message)
        {
            KernixCommercialExtraction result =
                new();

            if (string.IsNullOrWhiteSpace(message))
            {
                return result;
            }

            string text =
                message.Trim();

            string lower =
                text.ToLowerInvariant();


            // ========================================================
            // NOMBRE
            // ========================================================

            Match nameMatch =
                Regex.Match(
                    text,
                    @"\b(?:me llamo|mi nombre es|soy)\s+([A-Za-zÁÉÍÓÚáéíóúÑñ]{2,30})\b",
                    RegexOptions.IgnoreCase
                );

            if (nameMatch.Success)
            {
                result.NombreCliente =
                    nameMatch.Groups[1]
                        .Value
                        .Trim();
            }


            // ========================================================
            // TELÉFONO PERÚ
            // ========================================================

            Match phoneMatch =
                Regex.Match(
                    text,
                    @"(?<!\d)(?:\+?51[\s-]*)?(9\d{2}[\s-]?\d{3}[\s-]?\d{3})(?!\d)"
                );

            if (phoneMatch.Success)
            {
                result.Telefono =
                    Regex.Replace(
                        phoneMatch.Groups[1].Value,
                        @"\D",
                        string.Empty
                    );
            }


            // ========================================================
            // PRESUPUESTO
            // ========================================================

            Match budgetMatch =
                Regex.Match(
                    text,
                    @"(?:presupuesto(?:\s+es|\s+de)?|dispongo\s+de|tengo\s+un\s+presupuesto\s+de)\s*(?:s\/\.?|soles?)?\s*([\d,.]+)",
                    RegexOptions.IgnoreCase
                );

            if (budgetMatch.Success)
            {
                string raw =
                    budgetMatch.Groups[1]
                        .Value
                        .Trim();

                raw =
                    raw.Replace(",", string.Empty);

                if (
                    decimal.TryParse(
                        raw,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out decimal budget
                    )
                    &&
                    budget >= 0)
                {
                    result.Presupuesto =
                        budget;
                }
            }


            // ========================================================
            // PRODUCTO + CANTIDAD
            // ========================================================

            DetectProduct(
                text,
                lower,
                result
            );


            // ========================================================
            // USO
            // ========================================================

            if (
                lower.Contains("oficina") ||
                lower.Contains("administración") ||
                lower.Contains("administracion"))
            {
                result.Uso =
                    "Oficina / Administración";
            }
            else if (
                lower.Contains("programación") ||
                lower.Contains("programacion") ||
                lower.Contains("desarrollo"))
            {
                result.Uso =
                    "Desarrollo / Programación";
            }
            else if (
                lower.Contains("ventas") ||
                lower.Contains("vendedores"))
            {
                result.Uso =
                    "Ventas";
            }
            else if (
                lower.Contains("red") ||
                lower.Contains("infraestructura"))
            {
                result.Uso =
                    "Redes / Infraestructura";
            }

            return result;
        }


        // ============================================================
        // PRODUCTOS
        // ============================================================

        private static void DetectProduct(
            string original,
            string lower,
            KernixCommercialExtraction result)
        {
            if (
                lower.Contains("laptop") ||
                lower.Contains("laptops") ||
                lower.Contains("computadora") ||
                lower.Contains("computadoras"))
            {
                result.ProductId = 1;

                result.Cantidad =
                    ExtractQuantity(
                        original,
                        @"(?:laptops?|computadoras?)"
                    );

                return;
            }

            if (
                lower.Contains("monitor") ||
                lower.Contains("monitores"))
            {
                result.ProductId = 2;

                result.Cantidad =
                    ExtractQuantity(
                        original,
                        @"(?:monitores?|monitor)"
                    );

                return;
            }

            if (
                lower.Contains("teclado") ||
                lower.Contains("mouse") ||
                lower.Contains("kit"))
            {
                result.ProductId = 3;

                result.Cantidad =
                    ExtractQuantity(
                        original,
                        @"(?:kits?|teclados?|mouse)"
                    );

                return;
            }

            if (
                lower.Contains("router") ||
                lower.Contains("routers"))
            {
                result.ProductId = 4;

                result.Cantidad =
                    ExtractQuantity(
                        original,
                        @"routers?"
                    );

                return;
            }

            if (
                lower.Contains("switch") ||
                lower.Contains("switches"))
            {
                result.ProductId = 5;

                result.Cantidad =
                    ExtractQuantity(
                        original,
                        @"(?:switch|switches)"
                    );

                return;
            }

            if (
                lower.Contains("equipamiento de oficina") ||
                lower.Contains("equipar oficina"))
            {
                result.ProductId = 6;

                result.Cantidad = 1;
            }
        }


        // ============================================================
        // CANTIDAD
        // ============================================================

        private static int? ExtractQuantity(
            string text,
            string productPattern)
        {
            Match before =
                Regex.Match(
                    text,
                    $@"\b(\d{{1,3}})\s+{productPattern}\b",
                    RegexOptions.IgnoreCase
                );

            if (
                before.Success &&
                int.TryParse(
                    before.Groups[1].Value,
                    out int quantityBefore
                ))
            {
                return quantityBefore;
            }


            Match after =
                Regex.Match(
                    text,
                    $@"\b{productPattern}\s*(?:x|por|cantidad)?\s*(\d{{1,3}})\b",
                    RegexOptions.IgnoreCase
                );

            if (
                after.Success &&
                int.TryParse(
                    after.Groups[1].Value,
                    out int quantityAfter
                ))
            {
                return quantityAfter;
            }


            // Ejemplo:
            // "necesito laptops para 10 trabajadores"

            Match workers =
                Regex.Match(
                    text,
                    @"\b(?:para|somos)\s+(\d{1,3})\s+(?:trabajadores|personas|empleados|usuarios|puestos)\b",
                    RegexOptions.IgnoreCase
                );

            if (
                workers.Success &&
                int.TryParse(
                    workers.Groups[1].Value,
                    out int quantityWorkers
                ))
            {
                return quantityWorkers;
            }

            return null;
        }
    }
}