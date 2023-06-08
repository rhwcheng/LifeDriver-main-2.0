using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StyexFleetManagement.MZone.Client
{
    public interface IBaseClient
    {
        Task<HttpRequestMessage> CreateHttpRequestMessageAsync(CancellationToken cancellationToken);
    }
}