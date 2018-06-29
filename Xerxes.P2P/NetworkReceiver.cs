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
    //dotnet build
    //dotnet publish <csproj location> -c Release -r win-x64 -o <output location>
    //dotnet run --project <csproj location>
    public class NetworkReceiver
    {
        private static Random r = new Random();
        
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

        private bool infected = false;

        public NetworkReceiver(IPEndPoint localEndPoint)
        {
            this.LocalEndpoint = localEndPoint;
            this.tcpListener = new TcpListener(this.LocalEndpoint);
            this.tcpListener.Server.LingerState = new LingerOption(true, 0);
            this.tcpListener.Server.NoDelay = true;
            this.serverCancel = new CancellationTokenSource();            
            this.peers = new List<NetworkPeer>();                        
        }

        public void ReceivePeers(bool continuously)    
        {
            try
            {
                Console.WriteLine("Receiving peers on '{0}'.", this.LocalEndpoint);
                this.tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                this.tcpListener.Start();
                this.acceptTask = (continuously) ? this.AcceptClientsLoopAsync() : this.AcceptClientsSingleAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }      
                
        public async Task AcceptClientsLoopAsync()
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

        public async Task AcceptClientsSingleAsync()
        {
            try
            { 
                while(true)
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
                    var buffer = new byte[4096];
                    NetworkStream str = tcpClient.GetStream();
                    var byteCount = await str.ReadAsync(buffer, 0, buffer.Length);
                    var request = Encoding.UTF8.GetString(buffer, 0, byteCount);

                    System.Console.BackgroundColor = (System.ConsoleColor)r.Next(0, 14);
                    System.Console.Clear();
                    System.Console.WriteLine("Connection accepted from client '{0}'.", tcpClient.Client.RemoteEndPoint);
                    System.Console.WriteLine("INFECTED! Received message {0}", request);
                    if(!infected)
                    {
                        infected = true;
                        NetworkSeeker networkSeeker = new NetworkSeeker();
                        await Task.Run(() => networkSeeker.StartInfectPeersAsync(request, 1));
                    }
                    //tcpClient.Close();
                    System.Console.WriteLine("Close connection");
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }        


    }
}
