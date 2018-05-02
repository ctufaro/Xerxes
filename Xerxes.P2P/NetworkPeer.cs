using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Xerxes.P2P
{    
    public class NetworkPeer:INetworkPeer
    {
        public IPAddress ipAddr{ get;set;}
        public int port{get;set;}
        public NetworkPeer()
        {   
            
        }
        public NetworkPeer(string ipAddr, int port)
        {
            this.ipAddr = IPAddress.Parse(ipAddr);
            this.port = port;
        }
    }
}