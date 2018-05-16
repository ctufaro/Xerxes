using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Xerxes.P2P
{
    //dotnet publish <csproj location> -c Release -r win-x64 -o <output location>
    //dotnet run --project <csproj location>
    public class NetworkPeerServer
    {
        /// <summary>Cancellation that is triggered on shutdown to stop all pending operations.</summary>
        private readonly CancellationTokenSource serverCancel;

        /// <summary>TCP server listener accepting inbound connections.</summary>
        private readonly TcpListener tcpListener;

        /// <summary>IP address and port, on which the server listens to incoming connections.</summary>
        public IPEndPoint LocalEndpoint { get; private set; }

        public NetworkPeerServer(IPEndPoint localEndPoint)
        {
            this.LocalEndpoint = localEndPoint;
            this.tcpListener = new TcpListener(this.LocalEndpoint);
            this.tcpListener.Server.LingerState = new LingerOption(true, 0);
            this.tcpListener.Server.NoDelay = true;
            this.serverCancel = new CancellationTokenSource();
        }

        public async void Listen()
        {
            try
            {
                Console.WriteLine("Listening for clients on '{0}'.", this.LocalEndpoint);
                this.tcpListener.Start();
                while (!this.serverCancel.IsCancellationRequested)
                {
                    TcpClient tcpClient = await this.tcpListener.AcceptTcpClientAsync();
                    Console.WriteLine("Connection accepted from client '{0}'.", tcpClient.Client.RemoteEndPoint);
                    NetworkMessage networkMessage = new NetworkMessage(tcpClient.GetStream());
                    networkMessage.StartConversation();
                }
            }
            catch { }
        }
    }
}
