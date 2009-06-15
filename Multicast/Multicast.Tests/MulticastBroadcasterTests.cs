using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading;

namespace Multicast.Tests
{
    /// <summary>
    /// Summary description for MulticastBroadcasterTests
    /// </summary>
    [TestClass]
    public class MulticastBroadcasterTests
    {
        MulticastSettings testSettings = new MulticastSettings()
        {
            Address = IPAddress.Parse("239.1.2.3"),
            Port = 40404,
            TimeToLive = 0
        };

        [TestMethod]
        public void Constructor_WithSettingsAndFalse_NotBound()
        {
            // Arrange

            // Act
            IMulticastBroadcaster broadcaster = new MulticastBroadcaster(testSettings, false);

            // Assert
            Assert.IsInstanceOfType(broadcaster, typeof(IDisposable));
            Assert.AreEqual(testSettings, broadcaster.Settings);
            Assert.IsFalse(broadcaster.IsBound);
        }

        [TestMethod]
        public void Constructor_WithSettings_WillAutoBind()
        {
            // Arrange

            // Act
            IMulticastBroadcaster broadcaster = new MulticastBroadcaster(testSettings);

            // Assert
            Assert.AreEqual(testSettings, broadcaster.Settings);
            Assert.IsTrue(broadcaster.IsBound);
        }

        [TestMethod]
        public void Constructor_WithNullSettings_WillThrowArgumentNullException()
        {
            // Arrange
            MulticastSettings nullSettings = null;

            // Act - Assert
            try
            {
                IMulticastBroadcaster broadcaster = new MulticastBroadcaster(nullSettings);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("settings", ex.ParamName);
            }
        }

        [TestMethod]
        public void Dispose_WillDisconnectAndUnbind()
        {
            // Arrange
            IMulticastBroadcaster broadcaster = new MulticastBroadcaster(testSettings);

            // Act
            broadcaster.Dispose();

            // Assert
            Assert.IsFalse(broadcaster.IsBound);
        }

        [TestMethod]
        public void Broadcast_WillSendMessage_AsAMulticastBroadcast()
        {
            // Arrange
            string message = "test message";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            CallbackTester tester = new CallbackTester();

            using (IMulticastListener receiver = new MulticastListener(testSettings))
            {
                receiver.StartListening(tester.ReceiveCallback);

                // Act
                using (IMulticastBroadcaster broadcaster = new MulticastBroadcaster(testSettings))
                {
                    broadcaster.Broadcast(messageBytes);
                }
            }

            /*
             * try 5 times but break as soon as its not null
             * need to sleep for the async
             */
            int attempts = 1;
            do
            {
                if (tester.CallbackData.Length != 0) break;
                Thread.Sleep(250);
            }
            while (++attempts < 5);

            // Assert
            string actualMessage = Encoding.UTF8.GetString(tester.CallbackData);
            Assert.AreEqual(message, actualMessage);
        }
    }
}
