using System.Net;

namespace Multicast
{
    public sealed class MulticastSettings
    {
        public IPAddress Address { get; init; } = null!;
        public int Port { get; init; }
        public int TimeToLive { get; init; }
    }
}
