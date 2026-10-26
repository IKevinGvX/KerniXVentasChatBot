using KerniX.Web.Models;

namespace KerniX.Web.Services
{
    public interface IKernixActionService
    {
        KernixActionResponse DetectActions(string message);
    }
}