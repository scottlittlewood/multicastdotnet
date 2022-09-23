namespace Multicast.Tests.Unit
{
    public class CallbackTester
    {
        public byte[] CallbackData { get; private set; }

        public CallbackTester()
        {
            CallbackData = Array.Empty<byte>();
        }

        public void ReceiveCallback(byte[] data)
        {
            CallbackData = data;
        }
    }
}
