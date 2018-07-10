using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public NetworkPeerConnection(TcpClient client)
        {
            this.tcpClient = client;
            this.cancellationSource = new CancellationTokenSource();
            this.stream = this.tcpClient.GetStream();
        }
        
        public async Task<string> ReceiveMessageAsync()
        {
            try
            {
                var buffer = new byte[4096];
                var byteCount = await this.stream.ReadAsync(buffer, 0, buffer.Length);
                var request = Encoding.UTF8.GetString(buffer, 0, byteCount);
                System.Console.WriteLine("Received message {0} from {1}", request, this.tcpClient.Client.RemoteEndPoint);
                return request;
            }
            catch
            {
                System.Console.WriteLine("Connection Aborted");
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
            catch(Exception e)
            {               
                Console.WriteLine("Connection Aborted ({0})",e.ToString());
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
                System.Console.WriteLine("Sent message {0}", response);
            }
            catch(Exception e)
            {
                Console.WriteLine("Connection Aborted ({0})",e.ToString());
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
            catch(Exception e)
            {
                Console.WriteLine("Error: " + e.ToString());
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
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.ToString());
            }
        }

    }
    
}