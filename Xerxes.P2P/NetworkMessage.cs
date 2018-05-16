using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xerxes.P2P
{
    public class NetworkMessage
    {
        public NetworkStream networkStream { get; private set; }

        private string[] messages = new string[]{"Nice to meet you", "What do you hear", "Wow that is crazy"};

        private int count;

        public NetworkMessage(NetworkStream networkStream)
        {
            this.count = 0;
            this.networkStream = networkStream;
        }

        public async void StartConversation()
        {
            await Task.Run(async ()=>{
                while(true)
                {
                    await ReceiveClientMessage();
                    await SendClientMessage();
                }
            });
        }

        private async Task ReceiveClientMessage(){
            var buffer = new byte[4096];
            var byteCount = await this.networkStream.ReadAsync(buffer, 0, buffer.Length);
            var request = Encoding.UTF8.GetString(buffer, 0, byteCount);
            System.Console.WriteLine("[Server] Client wrote {0}", request);
        } 

        private async Task SendClientMessage(){
            if (count < messages.Length)
            {                
                string response = messages[count];
                count = Interlocked.Increment(ref count);
                byte[] serverResponseBytes = Encoding.UTF8.GetBytes(response);
                await this.networkStream.WriteAsync(serverResponseBytes, 0, serverResponseBytes.Length);
            }
        }      
    }

    public enum NetworkMessageType
    {
        /// <summary>Initial state of an outbound peer.</summary>
        Created = 0,
        /// <summary>Network connection with the peer has been established.</summary>
        Connected,
        /// <summary>The node and the peer exchanged version information.</summary>
        HandShaked,
        /// <summary>Process of disconnecting the peer has been initiated.</summary>
        Disconnecting,
        /// <summary>Shutdown has been initiated, the node went offline.</summary>
        Offline,
        /// <summary>An error occurred during a network operation.</summary>
        Failed
    }
}