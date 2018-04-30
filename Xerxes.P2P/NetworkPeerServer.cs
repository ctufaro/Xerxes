using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Xerxes.P2P
{
    public class NetworkPeerServer
    {
        private static TcpListener listener { get; set; }
        private static bool accept { get; set; } = false;

        public static void StartServer(string server, int port)
        {
            IPAddress address = IPAddress.Parse(server);
            listener = new TcpListener(address, port);
            listener.Server.LingerState = new LingerOption(true, 0);
            listener.Server.NoDelay = true;
            listener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            listener.Start();

            accept = true;

            Console.WriteLine($"Server started at {server}:{port}");
        }

        public static void Listen()
        {
            if (listener != null && accept)
            {

                // Continue listening.  
                while (true)
                {
                    Console.WriteLine("Listening for clients...");
                    var clientTask = listener.AcceptTcpClientAsync(); // Get the client  

                    if (clientTask.Result != null)
                    {
                        Console.WriteLine("Client connected. Waiting for data.");
                        var client = clientTask.Result;
                        string message = "";

                        while (message != null && !message.StartsWith("quit"))
                        {
                            byte[] data = Encoding.ASCII.GetBytes("Send next data: [enter 'quit' to terminate] ");
                            client.GetStream().Write(data, 0, data.Length);

                            byte[] buffer = new byte[1024];
                            client.GetStream().Read(buffer, 0, buffer.Length);

                            message = Encoding.ASCII.GetString(buffer);
                            Console.WriteLine(message);
                        }
                        Console.WriteLine("Closing connection.");
                        client.GetStream().Dispose();
                    }
                }
            }
        }
    }
}
