using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xerxes.Utils;

namespace Xerxes.P2P
{
    /// <summary>
    /// Represents a network connection to a peer. It is responsible for reading incoming messages 
    /// from the peer and sending messages from the node to the peer.
    /// </summary>
    public class NetworkPeerConnection
    {
        /// <summary>Underlying TCP client.</summary>
        public TcpClient tcpClient;

        /// <summary>Stream to send and receive messages through established TCP connection.</summary>
        /// <remarks>Write operations on the stream have to be protected by <see cref="writeLock"/>.</remarks>
        public NetworkStream stream;
        private readonly CancellationTokenSource cancellationSource;
        private INetworkConfiguration networkConfiguration;
        private UtilitiesConfiguration utilConf;
        private NetworkPeers foundPeers;     
        private NetworkPeers establishedPeers;    


        public NetworkPeerConnection(INetworkConfiguration networkConfiguration, NetworkPeers foundPeers, NetworkPeers establishedPeers, UtilitiesConfiguration utilConf)
        {
            this.cancellationSource = new CancellationTokenSource();
            this.networkConfiguration = networkConfiguration;
            this.utilConf = utilConf;
            this.foundPeers = foundPeers;
            this.establishedPeers = establishedPeers;
        }

        public NetworkPeerConnection(INetworkConfiguration networkConfiguration, NetworkPeers establishedPeers, UtilitiesConfiguration utilConf)
        {
            this.cancellationSource = new CancellationTokenSource();
            this.networkConfiguration = networkConfiguration;
            this.utilConf = utilConf;
            this.establishedPeers = establishedPeers;
        }               
        
        public async Task<string> ReceiveMessageAsync()
        {
            try
            {
                var buffer = new byte[4096];
                var byteCount = await this.stream.ReadAsync(buffer, 0, buffer.Length);
                var request = Encoding.UTF8.GetString(buffer, 0, byteCount);
                //Console.WriteLine("Received message {0} from {1}", request, this.tcpClient.Client.RemoteEndPoint);
                return request;
            }
            catch
            {
                //Console.WriteLine("Connection Aborted");
                return "";
            }
        }

        public async Task<NetworkMessage> GetMessageAsync()
        {
            try
            {
                var buffer = new byte[4096];
                var byteCount = await this.stream.ReadAsync(buffer, 0, buffer.Length);
                var json = Encoding.UTF8.GetString(buffer, 0, byteCount);
                return NetworkMessage.JSONToNetworkMessage(json);
            }
            catch(Exception)
            {               
                //Console.WriteLine("Connection Aborted ({0})",e.ToString());
                return null;                
            }
        }  

        public async Task SendMessageAsync()
        {
            try
            {
                string response = DateTime.Now.ToString();
                byte[] serverResponseBytes = Encoding.UTF8.GetBytes(response);
                await stream.WriteAsync(serverResponseBytes, 0, serverResponseBytes.Length);
                //Console.WriteLine("Sent message {0}", response);
            }
            catch(Exception)
            {
                //Console.WriteLine("Connection Aborted ({0})",e.ToString());
            }           
        }

        public async Task StartConversationAsync()
        {
            try
            {
                while (!this.cancellationSource.Token.IsCancellationRequested)
                {
                    Thread.Sleep(1000);
                    await SendMessageAsync();
                    await ReceiveMessageAsync();
                }
            }
            catch(Exception)
            {
                //Console.WriteLine("Error: " + e.ToString());
            }
        }

        public async Task ReceiveConversationAsync()
        {
            try
            {
                while (!this.cancellationSource.Token.IsCancellationRequested)
                {
                    Thread.Sleep(1000);
                    await SendMessageAsync();
                    await ReceiveMessageAsync();
                }
            }
            catch (Exception)
            {
                //Console.WriteLine("Error: " + e.ToString());
            }
        }


        private async Task BroadcastSeekAsync()
        {            
            foreach(NetworkPeer peer in foundPeers.peers.Values)
            {             
                //Console.WriteLine("Broadcasting to Peer: {0}", peer.IPEnd.ToString());
                using (var tcpClient = new TcpClient())
                {
                    await tcpClient.ConnectAsync(peer.IPEnd.Address, peer.IPEnd.Port);
                    //Console.WriteLine("Connected to peer, sending seek message..");
                    NetworkMessage nm = new NetworkMessage();
                    IPEndPoint myEndPoint = GetMyEndPoint();
                    nm.MessageSenderIP = myEndPoint.Address.ToString();
                    nm.MessageSenderPort = myEndPoint.Port;
                    nm.MessageStateType = NetworkStateType.Seek;
                    string json = NetworkMessage.NetworkMessageToJSON(nm);
                    byte[] bytes = Encoding.UTF8.GetBytes(json);
                    using (var networkStream = tcpClient.GetStream())
                    {
                        //Console.WriteLine("Sending to Peer {0}", json);
                        await networkStream.WriteAsync(bytes, 0, bytes.Length);
                    }                  

                }                
            }
        }

        public async Task BroadcastSingleSeekAsync(NetworkPeer peer)
        {            
            using (var tcpClient = new TcpClient())
            {                
                await tcpClient.ConnectAsync(peer.IPEnd.Address, peer.IPEnd.Port);                
                NetworkPeerMessage message = this.establishedPeers.AddOutboundPeer(peer);
                UtilitiesConsole.Update(UCommand.StatusOutbound, "Message status: " + message.ToString());
                UtilitiesConsole.Update(UCommand.OutboundPeers, this.establishedPeers.Count.ToString());
                NetworkMessage nm = new NetworkMessage();
                IPEndPoint myEndPoint = GetMyEndPoint();
                nm.MessageSenderIP = myEndPoint.Address.ToString();
                nm.MessageSenderPort = myEndPoint.Port;
                nm.MessageStateType = NetworkStateType.Seek;
                string json = NetworkMessage.NetworkMessageToJSON(nm);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                using (var networkStream = tcpClient.GetStream())
                {                   
                    await networkStream.WriteAsync(bytes, 0, bytes.Length);
                }
            }                
        }

        public IPEndPoint GetMyEndPoint()
        {
            if(networkConfiguration.Turf == Turf.Intranet)
            {
                int port = this.networkConfiguration.ReceivePort;
                return new IPEndPoint(IPAddress.Loopback, port); 
            }
            else if(networkConfiguration.Turf == Turf.TestNet)
            {
                int port = utilConf.GetOrDefault<int>("testnet",0);
                return new IPEndPoint(UtilitiesNetwork.GetMyIPAddress(), port); 
            }
            else 
            {
                int port = utilConf.GetOrDefault<int>("mainnet",0);
                return new IPEndPoint(UtilitiesNetwork.GetMyIPAddress(), port);
            } 
        }                    

    }
    
}