using System;
using System.Net;

namespace Xerxes.P2P.Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Loopback, 1234);
                NetworkPeerServer networkPeerServer = new NetworkPeerServer(iPEndPoint);
                networkPeerServer.Listen();
            }
            else
            {
                NetworkClient networkClient = new NetworkClient("127.0.0.1", 1234);
                networkClient.Start();
            }

            Console.ReadLine();
        }
    }
}
