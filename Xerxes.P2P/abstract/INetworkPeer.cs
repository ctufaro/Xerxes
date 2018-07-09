using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Xerxes.P2P
{
    public interface INetworkPeer
    {
        IPEndPoint IPEnd{ get;set;}
        string Id {get;set;}
        TcpClient TCPClient {get;set;}
    } 
}