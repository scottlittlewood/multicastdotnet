using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Multicast.Tests
{
    public class CallbackTester
    {
        public byte[] CallbackData { get; protected set; }

        public CallbackTester()
        {
            CallbackData = new byte[0];
        }

        public void ReceiveCallback(byte[] data)
        {
            CallbackData = data;
        }
    }
}
