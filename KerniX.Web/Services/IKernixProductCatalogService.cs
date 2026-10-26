using KerniX.Web.Models;

namespace KerniX.Web.Services
{
    public interface IKernixProductCatalogService
    {
        IReadOnlyList<KernixProduct> GetProducts();

        KernixProduct? GetProduct(int id);

        IReadOnlyList<KernixProduct> Search(
            string query
        );

        string BuildCatalogContext();
    }
}