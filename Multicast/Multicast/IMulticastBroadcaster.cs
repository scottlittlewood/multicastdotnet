using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Multicast
{
    public interface IMulticastBroadcaster : IDisposable
    {
        MulticastSettings Settings { get; }

        bool IsBound { get; }

        void Broadcast(byte[] data);
    }
}
