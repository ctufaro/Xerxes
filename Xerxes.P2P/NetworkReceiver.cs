using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xerxes.Utils;

namespace Xerxes.P2P
{
    public class NetworkReceiver
    {
        private static Random r = new Random();
        
        /// <summary>Cancellation that is triggered on shutdown to stop all pending operations.</summary>
        private readonly CancellationTokenSource serverCancel;

        private INetworkConfiguration networkConfiguration;

        private UtilitiesConfiguration utilConf;                

        /// <summary>TCP server listener accepting inbound connections.</summary>
        private readonly TcpListener tcpListener;      

        /// <summary>IP address and port, on which the server listens to incoming connections.</summary>
        public IPEndPoint LocalEndpoint { get; private set; }
        
        /// <summary>List of all inbound peers.</summary>
        private NetworkPeers Peers;
        /// <summary>Task accepting new clients in a loop.</summary>
        private Task acceptTask;

        public NetworkReceiver(IPEndPoint localEndPoint, INetworkConfiguration networkConfiguration, UtilitiesConfiguration utilConf)
        {
            this.LocalEndpoint = localEndPoint;
            this.tcpListener = new TcpListener(this.LocalEndpoint);
            this.tcpListener.Server.LingerState = new LingerOption(true, 0);
            this.tcpListener.Server.NoDelay = true;
            this.serverCancel = new CancellationTokenSource();
            this.networkConfiguration = networkConfiguration;
            this.utilConf = utilConf;            
            this.Peers = new NetworkPeers(utilConf.GetOrDefault<int>("maxinbound",117));                     
        }

        public void ReceivePeers()    
        {
            try
            {
                Console.WriteLine("Receiving peers on '{0}'.", this.LocalEndpoint);
                this.tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                this.tcpListener.Start();
                this.acceptTask = this.AcceptClientsAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }      
                
        public async Task AcceptClientsAsync()
        {
            try
            {                
                while (!this.serverCancel.IsCancellationRequested)
                {
                    TcpClient tcpClient = await Task.Run(() =>
                    {
                        try
                        {
                            Task<TcpClient> acceptClientTask = this.tcpListener.AcceptTcpClientAsync();
                            acceptClientTask.Wait(this.serverCancel.Token);
                            return acceptClientTask.Result;
                        }
                        catch (Exception exception)
                        {
                            // Record the error.
                            throw exception;
                        }
                    }).ConfigureAwait(false);
                    NetworkPeerConnection networkPeerConnection = new NetworkPeerConnection(tcpClient);
                    NetworkMessage message = await networkPeerConnection.GetMessageAsync();
                    IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(message.MessageSenderIP), message.MessageSenderPort);
                    NetworkPeer networkPeers = new NetworkPeer(ipEndPoint);
                    var result = this.Peers.AddInboundPeer(networkPeers);
                    Console.WriteLine("Status of Adding Peer: {0}", result.ToString());
                    Console.WriteLine("Inbound Peer Count: {0}", this.Peers.Count);
                }
            }
            catch { }
        }

    }
}
