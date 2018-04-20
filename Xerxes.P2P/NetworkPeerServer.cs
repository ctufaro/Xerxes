using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xerxes.Utilities;
using NBitcoin;
using NBitcoin.Protocol;

namespace Xerxes.P2P
{
    public class NetworkPeerServer : IDisposable
    {
        /// <summary>Instance logger.</summary>
        private readonly ILogger logger;

        /// <summary>Factory for creating P2P network peers.</summary>
        //private readonly INetworkPeerFactory networkPeerFactory;

        /// <summary>Specification of the network the node runs on - regtest/testnet/mainnet.</summary>
        public Network Network { get; private set; }

        /// <summary>Version of the protocol that the server is running.</summary>
        //public ProtocolVersion Version { get; private set; }

        /// <summary>The parameters that will be cloned and applied for each peer connecting to <see cref="NetworkPeerServer"/>.</summary>
        //public NetworkPeerConnectionParameters InboundNetworkPeerConnectionParameters { get; set; }

        /// <summary>Maximum number of inbound connection that the server is willing to handle simultaneously.</summary>
        /// <remarks>TODO: consider making this configurable.</remarks>
        public const int MaxConnectionThreshold = 125;

        /// <summary>IP address and port, on which the server listens to incoming connections.</summary>
        public IPEndPoint LocalEndpoint { get; private set; }

        /// <summary>IP address and port of the external network interface that is accessible from the Internet.</summary>
        public IPEndPoint ExternalEndpoint { get; private set; }

        /// <summary>TCP server listener accepting inbound connections.</summary>
        private readonly TcpListener tcpListener;

        /// <summary>Cancellation that is triggered on shutdown to stop all pending operations.</summary>
        private readonly CancellationTokenSource serverCancel;

        /// <summary>Maintains a list of connected peers and ensures their proper disposal.</summary>
        //private readonly NetworkPeerDisposer networkPeerDisposer;

        /// <summary>Task accepting new clients in a loop.</summary>
        private Task acceptTask;

        public NetworkPeerServer(Network network,
            IPEndPoint localEndPoint,
            IPEndPoint externalEndPoint,
            ProtocolVersion version,
            ILoggerFactory loggerFactory
            //,NetworkPeerFactory networkPeerFactory
            )
        {
            this.logger = loggerFactory.CreateLogger(this.GetType().FullName, $"[{localEndPoint}] ");
            Console.WriteLine("({0}:{1},{2}:{3},{4}:{5})", nameof(network), network, nameof(localEndPoint), localEndPoint, nameof(externalEndPoint), externalEndPoint, nameof(version), version);

            //this.networkPeerFactory = networkPeerFactory;
            //this.networkPeerDisposer = new NetworkPeerDisposer(loggerFactory);

            //this.InboundNetworkPeerConnectionParameters = new NetworkPeerConnectionParameters();

            this.LocalEndpoint = Utils.EnsureIPv6(localEndPoint);
            this.ExternalEndpoint = Utils.EnsureIPv6(externalEndPoint);

            this.Network = network;
            //this.Version = version;

            this.serverCancel = new CancellationTokenSource();

            this.tcpListener = new TcpListener(this.LocalEndpoint);
            this.tcpListener.Server.LingerState = new LingerOption(true, 0);
            this.tcpListener.Server.NoDelay = true;
            this.tcpListener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

            this.acceptTask = Task.CompletedTask;

            Console.WriteLine("Network peer server ready to listen on '{0}'.", this.LocalEndpoint);

            Console.WriteLine("(-)");
        }
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
