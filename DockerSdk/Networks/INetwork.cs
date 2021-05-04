using System.Threading;
using System.Threading.Tasks;

namespace DockerSdk.Networks
{
    public interface INetwork
    {
        NetworkFullId Id { get; }

        Task<INetworkInfo> GetDetailsAsync(CancellationToken ct = default);
    }
}
