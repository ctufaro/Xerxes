using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xerxes.P2P
{
    public class NetworkClient
    {
        public string Server { get; private set; }
        public int Port { get; private set; }
        private string[] messages = new string[] { "Hi server", "I hear Laurel", "Yeah cray cray" };
        private int count = 0;
        public IPEndPoint LocalEndpoint { get; private set; }

        public NetworkClient(IPEndPoint localEndPoint)
        {
            this.LocalEndpoint = localEndPoint;
        }

        public async void Start()
        {
            var tcpClient = new TcpClient();
            System.Console.WriteLine("[Client] Connecting to server");
            await tcpClient.ConnectAsync(this.LocalEndpoint.Address, this.LocalEndpoint.Port);
            System.Console.WriteLine("[Client] Connected to server, opening stream");

            await Task.Run(async () =>
            {
                NetworkStream stream = tcpClient.GetStream();
                while (true)
                {
                    await SendClientMessageAsync(stream);
                    Thread.Sleep(2000);
                    await ReceiveClientMessageAsync(stream);
                    Thread.Sleep(2000);
                }
            });
        }

        private async Task ReceiveClientMessageAsync(NetworkStream networkStream)
        {
            var buffer = new byte[4096];
            var byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length);
            var request = Encoding.UTF8.GetString(buffer, 0, byteCount);
            System.Console.WriteLine("[Server] Client wrote {0}", request);
        }

        private async Task SendClientMessageAsync(NetworkStream networkStream)
        {
            if (count < messages.Length)
            {
                string response = messages[count];
                count = Interlocked.Increment(ref count);
                byte[] serverResponseBytes = Encoding.UTF8.GetBytes(response);
                await networkStream.WriteAsync(serverResponseBytes, 0, serverResponseBytes.Length);
            }
        }        
    }
}
