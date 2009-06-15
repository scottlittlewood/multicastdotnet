using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Multicast
{
    public class MulticastSettings
    {
        public IPAddress Address { get; set; }
        public int Port { get; set; }
        public int TimeToLive { get; set; }
    }
}
