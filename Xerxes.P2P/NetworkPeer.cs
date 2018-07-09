using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Xerxes.P2P
{    
    public class NetworkPeer:INetworkPeer
    {
        public IPEndPoint IPEnd{ get;set;}
        public string Id {get;set;}
        public TcpClient TCPClient { get;set; }

        public NetworkPeer(IPEndPoint ipEnd, string id, TcpClient tcpClient)
        {
            this.IPEnd = ipEnd;
            this.Id = id;
            this.TCPClient = tcpClient;
        }

        public NetworkPeer(IPEndPoint ipEnd, string id)
        {
            this.IPEnd = ipEnd;
            this.Id = id;
        }

        public NetworkPeer(IPEndPoint ipEnd)
        {
            this.IPEnd = ipEnd;
        }        

        public NetworkPeer(TcpClient tcpClient)
        {           
            this.TCPClient = tcpClient;
        }

        public async void SendMessage(string response, NetworkStream receiver)
        {
            byte[] serverResponseBytes = Encoding.UTF8.GetBytes(response);
            await receiver.WriteAsync(serverResponseBytes, 0, serverResponseBytes.Length);
        }
    }

    public class NetworkPeers
    {
        public ConcurrentDictionary<string, NetworkPeer> peers;
        public NetworkPeers()
        {
            this.peers = new ConcurrentDictionary<string, NetworkPeer>();
        }

        public void AddPeer(NetworkPeer toBeAdded)
        {
            this.peers.TryAdd(toBeAdded.IPEnd.ToString(),toBeAdded);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if(this.peers.Count > 0)
            {
                foreach(var p in peers)
                {   
                    string output = (p.Value.IPEnd!=null) ? p.Value.IPEnd.ToString() : "";
                    sb.AppendLine(output);
                }
            }
            else
            {
                sb.AppendLine("No peers found.");
            }
            return sb.ToString();            
        }           
        
    }
}