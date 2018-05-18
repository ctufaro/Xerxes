using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xerxes.P2P
{
    //dotnet build
    //dotnet publish <csproj location> -c Release -r win-x64 -o <output location>
    //dotnet run --project <csproj location>
    public class NetworkReceiver
    {
        /// <summary>Cancellation that is triggered on shutdown to stop all pending operations.</summary>
        private readonly CancellationTokenSource serverCancel;
        

        /// <summary>TCP server listener accepting inbound connections.</summary>
        private readonly TcpListener tcpListener;      

        /// <summary>IP address and port, on which the server listens to incoming connections.</summary>
        public IPEndPoint LocalEndpoint { get; private set; }
        
        /// <summary>List of all inbound peers.</summary>
        private List<NetworkPeer> peers;
        /// <summary>Task accepting new clients in a loop.</summary>
        private Task acceptTask;

        public NetworkReceiver(IPEndPoint localEndPoint)
        {
            this.LocalEndpoint = localEndPoint;
            this.tcpListener = new TcpListener(this.LocalEndpoint);
            this.tcpListener.Server.LingerState = new LingerOption(true, 0);
            this.tcpListener.Server.NoDelay = true;
            this.serverCancel = new CancellationTokenSource();            
            this.peers = new List<NetworkPeer>();
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
                    Console.WriteLine("Connection accepted from client '{0}'.", tcpClient.Client.RemoteEndPoint);
                    this.peers.Add(new NetworkPeer(tcpClient));
                    NetworkPeerConnection networkPeerConnection = new NetworkPeerConnection(tcpClient);
                    Task conversation = networkPeerConnection.StartConversationAsync();
                }
            }
            catch { }
        }


    }
}
