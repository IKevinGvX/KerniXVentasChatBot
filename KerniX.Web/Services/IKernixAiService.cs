using KerniX.Web.Models;

namespace KerniX.Web.Services
{
    public interface IKernixAiService
    {
        Task<KernixChatResponse> SendMessageAsync(
            KernixChatRequest request,
            CancellationToken cancellationToken = default
        );

        IAsyncEnumerable<string> StreamMessageAsync(
            KernixChatRequest request,
            CancellationToken cancellationToken = default
        );
    }
}