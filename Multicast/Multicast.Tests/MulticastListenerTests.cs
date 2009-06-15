using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading;

namespace Multicast.Tests
{
    [TestClass]
    public class MulticastListenerTests
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
            IMulticastListener receiver = new MulticastListener(testSettings, false);

            // Assert
            Assert.IsInstanceOfType(receiver, typeof(IDisposable));
            Assert.AreEqual(testSettings, receiver.Settings);
            Assert.IsFalse(receiver.IsBound);
        }

        [TestMethod]
        public void Constructor_WithSettings_WillAutoBind()
        {
            // Arrange

            // Act
            IMulticastListener receiver = new MulticastListener(testSettings);

            // Assert
            Assert.AreEqual(testSettings, receiver.Settings);
            Assert.IsTrue(receiver.IsBound);

            receiver.Dispose();
        }

        [TestMethod]
        public void Constructor_WithNullSettings_WillThrowArgumentNullException()
        {
            // Arrange
            MulticastSettings nullSettings = null;

            // Act - Assert
            try
            {
                IMulticastListener receiver = new MulticastListener(nullSettings);
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
            IMulticastListener receiver = new MulticastListener(testSettings);

            // Act
            receiver.Dispose();

            // Assert
            Assert.IsFalse(receiver.IsBound);
        }

        /// <summary>
        /// currently use the other classes implementation to test it..
        /// hmm chicken and egg, should maybe use a direct socket/udpclient 
        /// to get the data from the multicast address but its a bit of a testing overhead to do so
        /// </summary>
        [TestMethod]
        public void StartListening_WillCallback_WhenDataISSentOnTheMulticastAddress()
        {
            // Arrange
            string message = "test message";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            CallbackTester tester = new CallbackTester();

            // Act
            using (IMulticastListener receiver = new MulticastListener(testSettings))
            {
                receiver.StartListening(tester.ReceiveCallback);
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

        [TestMethod]
        public void StopListening_WillNotCallback()
        {
            // Arrange
            string message = "test message";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            CallbackTester tester = new CallbackTester();

            // Act
            IMulticastListener receiver = new MulticastListener(testSettings);
            receiver.StartListening(tester.ReceiveCallback);
            receiver.StopListening();

            using (IMulticastBroadcaster broadcaster = new MulticastBroadcaster(testSettings))
            {
                broadcaster.Broadcast(messageBytes);
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
            Assert.AreEqual(string.Empty, actualMessage);
        }
    }
}
