using System.Globalization;
using System.Text;
using KerniX.Web.Models;

namespace KerniX.Web.Services
{
    public class KernixProductCatalogService
        : IKernixProductCatalogService
    {
        private readonly IReadOnlyList<KernixProduct>
            _products;

        public KernixProductCatalogService()
        {
            _products =
                new List<KernixProduct>
                {
                    new()
                    {
                        Id = 1,

                        Name =
                            "Laptop Empresarial Pro",

                        Code =
                            "KX-LAP-001",

                        Category =
                            "Computación",

                        Description =
                            "Laptop orientada al trabajo empresarial y productividad.",

                        Price =
                            3499m,

                        PriceText =
                            "S/ 3,499",

                        Url =
                            "/Productos/Detalle?id=1",

                        RecommendedFor =
                            new()
                            {
                                "oficina",
                                "trabajo empresarial",
                                "productividad",
                                "trabajo administrativo"
                            },

                        Keywords =
                            new()
                            {
                                "laptop",
                                "portátil",
                                "computadora",
                                "equipo",
                                "oficina"
                            }
                    },


                    new()
                    {
                        Id = 2,

                        Name =
                            "Monitor Profesional",

                        Code =
                            "KX-MON-002",

                        Category =
                            "Monitores",

                        Description =
                            "Monitor orientado a estaciones de trabajo profesionales.",

                        Price =
                            899m,

                        PriceText =
                            "S/ 899",

                        Url =
                            "/Productos/Detalle?id=2",

                        RecommendedFor =
                            new()
                            {
                                "oficina",
                                "productividad",
                                "estación de trabajo",
                                "segunda pantalla"
                            },

                        Keywords =
                            new()
                            {
                                "monitor",
                                "pantalla",
                                "display",
                                "oficina"
                            }
                    },


                    new()
                    {
                        Id = 3,

                        Name =
                            "Kit Teclado y Mouse",

                        Code =
                            "KX-ACC-003",

                        Category =
                            "Accesorios",

                        Description =
                            "Kit de teclado y mouse para puestos de trabajo.",

                        Price =
                            149m,

                        PriceText =
                            "S/ 149",

                        Url =
                            "/Productos/Detalle?id=3",

                        RecommendedFor =
                            new()
                            {
                                "oficina",
                                "puesto de trabajo",
                                "accesorios"
                            },

                        Keywords =
                            new()
                            {
                                "teclado",
                                "mouse",
                                "ratón",
                                "accesorio",
                                "kit"
                            }
                    },


                    new()
                    {
                        Id = 4,

                        Name =
                            "Router Empresarial",

                        Code =
                            "KX-NET-004",

                        Category =
                            "Redes",

                        Description =
                            "Router orientado a conectividad para entornos empresariales.",

                        Price =
                            599m,

                        PriceText =
                            "S/ 599",

                        Url =
                            "/Productos/Detalle?id=4",

                        RecommendedFor =
                            new()
                            {
                                "empresa",
                                "oficina",
                                "conectividad",
                                "red empresarial"
                            },

                        Keywords =
                            new()
                            {
                                "router",
                                "internet",
                                "wifi",
                                "red",
                                "conectividad"
                            }
                    },


                    new()
                    {
                        Id = 5,

                        Name =
                            "Switch de Red",

                        Code =
                            "KX-NET-005",

                        Category =
                            "Redes",

                        Description =
                            "Switch para interconectar equipos dentro de una red empresarial.",

                        Price =
                            449m,

                        PriceText =
                            "S/ 449",

                        Url =
                            "/Productos/Detalle?id=5",

                        RecommendedFor =
                            new()
                            {
                                "red empresarial",
                                "oficina",
                                "múltiples equipos",
                                "infraestructura"
                            },

                        Keywords =
                            new()
                            {
                                "switch",
                                "ethernet",
                                "red",
                                "lan",
                                "conexiones"
                            }
                    },


                    new()
                    {
                        Id = 6,

                        Name =
                            "Equipamiento de Oficina",

                        Code =
                            "KX-OFF-006",

                        Category =
                            "Soluciones empresariales",

                        Description =
                            "Solución de equipamiento empresarial configurable según las necesidades del cliente.",

                        Price =
                            null,

                        PriceText =
                            "Consultar",

                        Url =
                            "/Productos/Detalle?id=6",

                        RecommendedFor =
                            new()
                            {
                                "implementación de oficina",
                                "empresa",
                                "equipamiento integral",
                                "proyecto empresarial"
                            },

                        Keywords =
                            new()
                            {
                                "oficina",
                                "equipamiento",
                                "empresa",
                                "implementación",
                                "solución"
                            }
                    }
                };
        }


        public IReadOnlyList<KernixProduct>
            GetProducts()
        {
            return _products;
        }


        public KernixProduct? GetProduct(
            int id
        )
        {
            return _products.FirstOrDefault(
                product =>
                    product.Id == id
            );
        }


        public IReadOnlyList<KernixProduct> Search(
            string query
        )
        {
            if (string.IsNullOrWhiteSpace(query))
                return Array.Empty<KernixProduct>();


            string normalized =
                query.Trim();


            return _products
                .Where(
                    product =>

                        product.Name.Contains(
                            normalized,
                            StringComparison.OrdinalIgnoreCase
                        )

                        ||

                        product.Category.Contains(
                            normalized,
                            StringComparison.OrdinalIgnoreCase
                        )

                        ||

                        product.Code.Contains(
                            normalized,
                            StringComparison.OrdinalIgnoreCase
                        )

                        ||

                        product.Keywords.Any(
                            keyword =>
                                normalized.Contains(
                                    keyword,
                                    StringComparison.OrdinalIgnoreCase
                                )

                                ||

                                keyword.Contains(
                                    normalized,
                                    StringComparison.OrdinalIgnoreCase
                                )
                        )
                )
                .ToList();
        }


        public string BuildCatalogContext()
        {
            StringBuilder builder =
                new();


            builder.AppendLine(
                "CATÁLOGO COMERCIAL OFICIAL KERNIX:"
            );


            foreach (
                KernixProduct product
                in _products
            )
            {
                builder.AppendLine();

                builder.AppendLine(
                    $"ID: {product.Id}"
                );

                builder.AppendLine(
                    $"Producto: {product.Name}"
                );

                builder.AppendLine(
                    $"Código: {product.Code}"
                );

                builder.AppendLine(
                    $"Categoría: {product.Category}"
                );

                builder.AppendLine(
                    $"Descripción: {product.Description}"
                );

                builder.AppendLine(
                    $"Precio publicado: {product.PriceText}"
                );

                builder.AppendLine(
                    $"Página: {product.Url}"
                );

                builder.AppendLine(
                    "Recomendado para: " +
                    string.Join(
                        ", ",
                        product.RecommendedFor
                    )
                );
            }


            builder.AppendLine();

            builder.AppendLine(
                "REGLAS DEL CATÁLOGO:"
            );

            builder.AppendLine(
                "- No inventes productos que KerniX no tenga en este catálogo."
            );

            builder.AppendLine(
                "- No inventes precios."
            );

            builder.AppendLine(
                "- No afirmes disponibilidad o stock porque este catálogo no contiene inventario en tiempo real."
            );

            builder.AppendLine(
                "- Si el precio dice 'Consultar', no inventes un precio."
            );

            builder.AppendLine(
                "- Puedes recomendar combinaciones de productos cuando sean útiles para la necesidad del cliente."
            );


            return builder.ToString();
        }
    }
}