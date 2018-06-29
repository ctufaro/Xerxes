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
        /// <summary>List of all outbound peers.</summary>
        private List<NetworkPeer> peers;

        private IPEndPoint SingleSeekPoint;

        private static readonly int[] KnownPeers = new int[]{ 1230, 1231, 1232, 1233, 1234, 1235, 1236, 1237, 1238, 1239, 1240, 1241, 1242, 1243, 1244, 1245, 1246, 1247, 1248, 1249, 1250, 1251, 1252, 1253, 1254 };

        private ConcurrentDictionary<int,NetworkStream> visitedPorts;

        private static Random random = new Random();

        public NetworkSeeker(IPEndPoint singleSeekPoint)
        {
            this.SingleSeekPoint = singleSeekPoint;
            this.serverCancel = new CancellationTokenSource();
            this.peers = new List<NetworkPeer>();
            
        }

        public NetworkSeeker()
        {
            visitedPorts = new ConcurrentDictionary<int, NetworkStream>();
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

        private async Task SendGossipAsync(string message, int port)
        {
            try
            {   
                
                if(!visitedPorts.ContainsKey(port))
                {                
                    using (var tcpClient = new TcpClient())
                    {
                        await tcpClient.ConnectAsync(IPAddress.Loopback, port);
                        await Task.Run(async () =>
                        {
                            NetworkStream st = tcpClient.GetStream();
                            visitedPorts.TryAdd(port,st);
                            byte[] serverResponseBytes = Encoding.UTF8.GetBytes(message);
                            await st.WriteAsync(serverResponseBytes, 0, serverResponseBytes.Length);
                        });
                    }                
                }
                else
                {
                    await Task.Run(async () =>
                    {                        
                        byte[] serverResponseBytes = Encoding.UTF8.GetBytes(message);
                        await visitedPorts[port].WriteAsync(serverResponseBytes, 0, serverResponseBytes.Length);
                    });
                }
            }
            catch { }
        }

        private IEnumerable<int> RandomPeerIndex(int n)
        {
            for(int i = 1; i <= n; i++)
            {
                yield return random.Next(0, KnownPeers.Length);
            }
        }

        public async Task StartInfectPeersAsync(string message, int intervalSeconds)
        {           
            while(true)
            {
                int fanout = 1;//random.Next(1, KnownPeers.Length);
                foreach(int i in RandomPeerIndex(fanout))
                {
                    await SendGossipAsync(message, KnownPeers[i]);
                }
                Thread.Sleep(intervalSeconds*1000);
            }
        }

    }
}
