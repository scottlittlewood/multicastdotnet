using System.Net;
using System.Text;

namespace Multicast.Tests.Unit
{
    /// <summary>
    /// Summary description for MulticastBroadcasterTests
    /// </summary>
    public class MulticastBroadcasterTests
    {
        private readonly MulticastSettings _testSettings = new()
        {
            Address = IPAddress.Parse("239.1.2.3"),
            Port = 40404,
            TimeToLive = 0
        };

        [Fact]
        public void Constructor_WithSettingsAndFalse_NotBound()
        {
            // Arrange

            // Act
            IMulticastBroadcaster broadcaster = new MulticastBroadcaster(_testSettings, false);

            // Assert
            Assert.IsAssignableFrom<IDisposable>(broadcaster);
            Assert.Equal(_testSettings, broadcaster.Settings);
            Assert.False(broadcaster.IsBound);
        }

        [Fact]
        public void Constructor_WithSettings_WillAutoBind()
        {
            // Arrange

            // Act
            IMulticastBroadcaster broadcaster = new MulticastBroadcaster(_testSettings);

            // Assert
            Assert.Equal(_testSettings, broadcaster.Settings);
            Assert.True(broadcaster.IsBound);
        }

        [Fact]
        public void Constructor_WithNullSettings_WillThrowArgumentNullException()
        {
            // Arrange
            MulticastSettings nullSettings = null;

            // Act - Assert
            Assert.Throws<ArgumentNullException>(() => new MulticastBroadcaster(nullSettings));
        }

        [Fact]
        public void Dispose_WillDisconnectAndUnbind()
        {
            // Arrange
            IMulticastBroadcaster broadcaster = new MulticastBroadcaster(_testSettings);

            // Act
            broadcaster.Dispose();

            // Assert
            Assert.False(broadcaster.IsBound);
        }

        [Fact]
        public void Broadcast_WillSendMessage_AsAMulticastBroadcast()
        {
            // Arrange
            var message = "test message";
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var tester = new CallbackTester();

            using (IMulticastListener receiver = new MulticastListener(_testSettings))
            {
                receiver.StartListening(tester.ReceiveCallback);

                // Act
                using (IMulticastBroadcaster broadcaster = new MulticastBroadcaster(_testSettings))
                {
                    broadcaster.Broadcast(messageBytes);
                }
            }

            /*
             * try 5 times but break as soon as its not null
             * need to sleep for the async
             */
            var attempts = 1;
            do
            {
                if (tester.CallbackData.Length != 0) break;
                Thread.Sleep(250);
            }
            while (++attempts < 5);

            // Assert
            var actualMessage = Encoding.UTF8.GetString(tester.CallbackData);
            Assert.Equal(message, actualMessage);
        }
    }
}
