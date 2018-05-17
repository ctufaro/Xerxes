using System;
using System.Net;
using System.Threading.Tasks;

namespace Xerxes.P2P.Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Loopback, 1234);
                NetworkPeerServer networkPeerServer = new NetworkPeerServer(iPEndPoint);
                Task.Run(()=>networkPeerServer.ReceivePeers());
                
                //Task.Run(()=>networkPeerServer.SeekPeers());
            }
            else
            {
                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Loopback, 1234);
                NetworkClient networkClient = new NetworkClient(iPEndPoint);
                networkClient.Start();
            }

            Console.ReadLine();
        }
    }
}
