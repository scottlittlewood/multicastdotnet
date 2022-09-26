namespace Multicast
{
    public delegate void ReceiveHandler(byte[] data);

    public interface IMulticastListener : IDisposable
    {
        event ReceiveHandler OnReceive;

        MulticastSettings Settings { get; }

        bool IsBound { get; }

        void StartListening(ReceiveHandler handler);
        void StopListening();
    }
}
