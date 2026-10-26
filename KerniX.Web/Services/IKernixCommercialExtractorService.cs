using KerniX.Web.Models;

namespace KerniX.Web.Services
{
    public interface IKernixCommercialExtractorService
    {
        KernixCommercialExtraction Extract(
            string message
        );
    }
}