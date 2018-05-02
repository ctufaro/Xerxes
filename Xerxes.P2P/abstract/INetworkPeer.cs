using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Xerxes.P2P
{
    public interface INetworkPeer
    {
        IPAddress ipAddr{ get;set; }
        int port{ get;set; } 
    } 
}