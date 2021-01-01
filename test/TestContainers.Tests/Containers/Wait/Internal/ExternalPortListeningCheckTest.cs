using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NSubstitute;
using TestContainers.Containers;
using TestContainers.Containers.WaitStrategies;
using Xunit;

namespace TestContainers.Tests.Containers.Wait.Internal
{
    public class ExternalPortListeningCheckTest : IDisposable
    {
        private TcpListener _listeningSocket2;
        private TcpListener _nonListeningSocket;
        private IWaitStrategyTarget _mockContainer;
        private TcpListener _listeningSocket1;

        public ExternalPortListeningCheckTest()
        {

            _listeningSocket1 = new TcpListener(IPAddress.Loopback, 0);
            _listeningSocket1.Start();
            _listeningSocket2 = new TcpListener(IPAddress.Loopback, 0);
            _listeningSocket2.Start();

            _nonListeningSocket = new TcpListener(IPAddress.Loopback, 0);
            //nonListeningSocket.close();

            _mockContainer = Substitute.For<IWaitStrategyTarget>();
            _mockContainer.Host.Returns("127.0.0.1");
        }

        [Fact]
        public void SingleListening()
        {

            var check = new ExternalPortListeningCheck(_mockContainer, new List<int> { ((IPEndPoint) _listeningSocket1.LocalEndpoint).Port });

            var result = check.Check();

            Assert.True(result, "ExternalPortListeningCheck identifies a single listening port");
        }


        [Fact]
        public void multipleListening()
        {

            var check = new ExternalPortListeningCheck(_mockContainer, new List<int> { ((IPEndPoint) _listeningSocket1.LocalEndpoint).Port, ((IPEndPoint) _listeningSocket2.LocalEndpoint).Port });

            var result = check.Check();

            Assert.True(result, "ExternalPortListeningCheck identifies multiple listening port");
        }

        [Fact]
        public void OneNotListening()
        {

            var check = new ExternalPortListeningCheck(_mockContainer, new List<int> { ((IPEndPoint) _listeningSocket1.LocalEndpoint).Port, ((IPEndPoint) _nonListeningSocket.LocalEndpoint).Port });

            Assert.Throws<IllegalStateException>(() => check.Check()); // "ExternalPortListeningCheck detects a non-listening port among many"
        }

        public void Dispose()
        {
            _listeningSocket2.Stop();
            _listeningSocket2.Stop();
        }
    }


}
