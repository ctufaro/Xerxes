using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Xerxes.P2P
{
    public class NetworkClient
    {
        public string Server { get; private set; }
        public int Port { get; private set; }
        public NetworkClient(string server, int port)
        {
            this.Server = server;
            this.Port = port;
        }

        public async void Start()
        {
            using (var tcpClient = new TcpClient())
            {
                Console.WriteLine("[Client] Connecting to server");
                await tcpClient.ConnectAsync(this.Server, this.Port);
                Console.WriteLine("[Client] Connected to server");
                //using (var networkStream = tcpClient.GetStream())
                //{
                //    Console.WriteLine("[Client] Writing request {0}", ClientRequestString);
                //    await networkStream.WriteAsync(ClientRequestBytes, 0, ClientRequestBytes.Length);

                //    var buffer = new byte[4096];
                //    var byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                //    var response = Encoding.UTF8.GetString(buffer, 0, byteCount);
                //    Console.WriteLine("[Client] Server response was {0}", response);
                //}
            }
        }
    }
}
