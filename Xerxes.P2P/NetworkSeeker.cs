using System;
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
        /// <summary>List of all outbound peers.</summary>
        private List<NetworkPeer> peers;

        private IPEndPoint SingleSeekPoint;

        public NetworkSeeker(IPEndPoint singleSeekPoint)
        {
            this.SingleSeekPoint = singleSeekPoint;
            this.serverCancel = new CancellationTokenSource();
            this.peers = new List<NetworkPeer>();
        }

        public void SeekPeers()
        {
            SeekPeersAsync();
        }

        public async void SeekPeersAsync()
        {
            try
            {
                Console.WriteLine("Seeking peer on '{0}'.", this.SingleSeekPoint);
                while (!this.serverCancel.IsCancellationRequested)
                {
                    using (var tcpClient = new TcpClient())
                    {
                        await tcpClient.ConnectAsync(this.SingleSeekPoint.Address, this.SingleSeekPoint.Port);
                        peers.Add(new NetworkPeer(tcpClient));
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
