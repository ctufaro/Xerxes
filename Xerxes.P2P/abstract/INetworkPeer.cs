using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Xerxes.P2P
{
    public interface INetworkPeer
    {
        IPEndPoint ipEnd{ get;set;}
        string id {get;set;}
        TcpClient tcpClient {get;set;}
    } 
}