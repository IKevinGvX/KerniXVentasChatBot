using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KerniX.Web.Pages.Cotizacion
{
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? Producto { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Cantidad { get; set; } = 1;

        public void OnGet()
        {
            if (Cantidad < 1)
            {
                Cantidad = 1;
            }

            if (Cantidad > 99)
            {
                Cantidad = 99;
            }
        }
    }
}