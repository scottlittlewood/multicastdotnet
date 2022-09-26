using System.Net.Sockets;
using System.Net;

namespace Multicast
{
    public sealed class MulticastListener : IMulticastListener
    {
        public event ReceiveHandler OnReceive;

        public MulticastSettings Settings { get; protected set; }

        public bool IsBound
        {
            get
            {
                return UdpClient.Client != null
                        ? UdpClient.Client.IsBound
                        : false;
            }
        }

        private UdpClient udpClient;
        public UdpClient UdpClient
        {
            get { return udpClient ?? (udpClient = new UdpClient()); }
        }

        public IPEndPoint LocalIPEndPoint { get; protected set; }

        public MulticastListener(MulticastSettings settings)
            : this(settings, true)
        { }

        public MulticastListener(MulticastSettings settings, bool autoBindJoinConnect)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            Settings = settings;

            if (autoBindJoinConnect) BindAndJoin();
        }

        private void BindAndJoin()
        {
            try
            {
                LocalIPEndPoint = new IPEndPoint(IPAddress.Any, Settings.Port);
                UdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                UdpClient.ExclusiveAddressUse = false;
                UdpClient.EnableBroadcast = true;

                UdpClient.Client.Bind(LocalIPEndPoint);
                UdpClient.JoinMulticastGroup(Settings.Address, Settings.TimeToLive);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void StartListening(ReceiveHandler handler)
        {
            try
            {
                if (!IsBound) BindAndJoin();

                OnReceive += handler;
                AsyncCallback receiveCallback = new AsyncCallback(ReceiveCallback);
                UdpClient.BeginReceive(receiveCallback, this);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void StopListening()
        {
            OnReceive = null;
            if(IsBound) UnbindAndLeave();
        }

        private void UnbindAndLeave()
        {
            try
            {
                UdpClient.DropMulticastGroup(Settings.Address);
                UdpClient.Close();
            }
            catch (ObjectDisposedException)
            {
                // expected exception fired when we close - swallow it up
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                MulticastListener receiver = (MulticastListener)(ar.AsyncState);

                UdpClient udpClient = receiver.UdpClient;
                IPEndPoint ipEndPoint = receiver.LocalIPEndPoint;

                byte[] receiveBytes = udpClient.EndReceive(ar, ref ipEndPoint);
                OnReceive(receiveBytes);

                AsyncCallback receiveCallback = new AsyncCallback(ReceiveCallback);
                UdpClient.BeginReceive(receiveCallback, this);
            }
            catch (ObjectDisposedException)
            {
                // expected exception fired when we close - swallow it up
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Dispose()
        {
            if (IsBound) UnbindAndLeave();
        }
    }
}
