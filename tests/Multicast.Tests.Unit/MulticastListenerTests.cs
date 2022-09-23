using System.Net;
using System.Text;

namespace Multicast.Tests.Unit
{
    public class MulticastListenerTests
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
            IMulticastListener receiver = new MulticastListener(_testSettings, false);

            // Assert
            Assert.IsAssignableFrom<IDisposable>(receiver);
            Assert.Equal(_testSettings, receiver.Settings);
            Assert.False(receiver.IsBound);
        }

        [Fact]
        public void Constructor_WithSettings_WillAutoBind()
        {
            // Arrange

            // Act
            IMulticastListener receiver = new MulticastListener(_testSettings);

            // Assert
            Assert.Equal(_testSettings, receiver.Settings);
            Assert.True(receiver.IsBound);

            receiver.Dispose();
        }

        [Fact]
        public void Constructor_WithNullSettings_WillThrowArgumentNullException()
        {
            // Arrange
            MulticastSettings? nullSettings = null;

            // Act - Assert
            try
            {
                IMulticastListener receiver = new MulticastListener(nullSettings);
                Assert.True(false, "Should not get here");
            }
            catch (ArgumentNullException ex)
            {
                Assert.Equal("settings", ex.ParamName);
            }
        }

        [Fact]
        public void Dispose_WillDisconnectAndUnbind()
        {
            // Arrange
            IMulticastListener receiver = new MulticastListener(_testSettings);

            // Act
            receiver.Dispose();

            // Assert
            Assert.False(receiver.IsBound);
        }

        /// <summary>
        /// currently use the other classes implementation to test it..
        /// hmm chicken and egg, should maybe use a direct socket/udpclient 
        /// to get the data from the multicast address but its a bit of a testing overhead to do so
        /// </summary>
        [Fact]
        public void StartListening_WillCallback_WhenDataISSentOnTheMulticastAddress()
        {
            // Arrange
            var message = "test message";
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var tester = new CallbackTester();

            // Act
            using (IMulticastListener receiver = new MulticastListener(_testSettings))
            {
                receiver.StartListening(tester.ReceiveCallback);
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

        [Fact]
        public void StopListening_WillNotCallback()
        {
            // Arrange
            var message = "test message";
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var tester = new CallbackTester();

            // Act
            IMulticastListener receiver = new MulticastListener(_testSettings);
            receiver.StartListening(tester.ReceiveCallback);
            receiver.StopListening();

            using (IMulticastBroadcaster broadcaster = new MulticastBroadcaster(_testSettings))
            {
                broadcaster.Broadcast(messageBytes);
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
            Assert.Equal(string.Empty, actualMessage);
        }
    }
}
