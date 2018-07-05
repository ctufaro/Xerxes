using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Xerxes.P2P
{
    public class NetworkSeeker
    {
        /// <summary>Cancellation that is triggered on shutdown to stop all pending operations.</summary>
        private readonly CancellationTokenSource serverCancel;
        
        private INetworkConfiguration networkConfiguration;

        public NetworkSeeker(INetworkConfiguration networkConfiguration)
        {
            this.serverCancel = new CancellationTokenSource();         
            this.networkConfiguration = networkConfiguration;
        }

        public async void SeekPeersAsync(IPEndPoint singleSeekPoint = null)
        {
            try
            {
                if(singleSeekPoint==null)
                {
                    //lets discover some peers
                    NetworkDiscovery networkDiscovery = new NetworkDiscovery(networkConfiguration);
                    singleSeekPoint = networkDiscovery.ConnectToStreet();
                }

                Console.WriteLine("Seeking peer on '{0}'.", singleSeekPoint);              
                while (!this.serverCancel.IsCancellationRequested)
                {
                    using (var tcpClient = new TcpClient())
                    {
                        await tcpClient.ConnectAsync(singleSeekPoint.Address, singleSeekPoint.Port);
                        //peers.Add(new NetworkPeer(tcpClient));
                        System.Console.WriteLine("Connected to peer, opening stream..");

                        await Task.Run(() =>
                        {
                            NetworkPeerConnection networkPeerConnection = new NetworkPeerConnection(tcpClient);
                            while (true)
                            {
                                Thread.Sleep(1000);
                                Task conversation = networkPeerConnection.ReceiveConversationAsync();
                            }
                        });
                    }
                }
            }
            catch { }
        } 

               

    }
}
