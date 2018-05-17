using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Xerxes.P2P
{    
    public class NetworkPeer:INetworkPeer
    {
        public IPEndPoint ipEnd{ get;set;}
        public string id {get;set;}
        public TcpClient tcpClient { get;set; }

        public NetworkPeer(IPEndPoint ipEnd, string id, TcpClient tcpClient)
        {
            this.ipEnd = ipEnd;
            this.id = id;
            this.tcpClient = tcpClient;
        }

        public NetworkPeer(TcpClient tcpClient)
        {           
            this.tcpClient = tcpClient;
        }

        public async void SendMessage(string response, NetworkStream receiver)
        {
            byte[] serverResponseBytes = Encoding.UTF8.GetBytes(response);
            await receiver.WriteAsync(serverResponseBytes, 0, serverResponseBytes.Length);
        }
    }
}