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
            System.Console.WriteLine("[Client] Connected to server, opening stream..");

            await Task.Run(async () =>
            {
                NetworkPeerConnection networkPeerConnection = new NetworkPeerConnection(tcpClient);
                while (true)
                {
                    Thread.Sleep(1000);
                    await networkPeerConnection.SendMessageAsync();
                    string request = await networkPeerConnection.ReceiveMessageAsync();
                    if(request.Contains("---"))
                    {
                        string[] reqs = request.Split(new string[]{"---"}, StringSplitOptions.RemoveEmptyEntries);
                        if(reqs[0].Trim().Equals("JOIN"))
                        {
                            string host = reqs[1];
                            string port = reqs[2];
                            Console.WriteLine("Create a socket and chat to {0}:{1}", host, port);
                        }
                    }
                }
            });
        }
  
    }
}
