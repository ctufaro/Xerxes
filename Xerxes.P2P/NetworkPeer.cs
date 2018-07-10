using System;
using System.Collections;
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

        public NetworkPeer(IPEndPoint ipEnd)
        {
            this.IPEnd = ipEnd;
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
        public int maxinbound{get;set;}

        public int Count
        {
            get
            {
                return this.peers.Count;
            }
        }
        public NetworkPeers()
        {
            this.peers = new ConcurrentDictionary<string, NetworkPeer>();
        }

        public NetworkPeers(int maxinbound)
        {
            this.peers = new ConcurrentDictionary<string, NetworkPeer>();
            this.maxinbound = maxinbound;
        }        

        public void AddPeer(NetworkPeer toBeAdded)
        {
            this.peers.TryAdd(toBeAdded.IPEnd.ToString(),toBeAdded);
        }

        public NetworkPeerMessage AddInboundPeer(NetworkPeer toBeAdded)
        {
            if(this.peers.Count < maxinbound)
            {
                bool result =  this.peers.TryAdd(toBeAdded.IPEnd.ToString(),toBeAdded);
                if(result)
                    return NetworkPeerMessage.Success;
                else
                    return NetworkPeerMessage.AlreadyExists;
            }
            else
            {
                return NetworkPeerMessage.MaximumConnectionsReached;
            }
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

    public enum NetworkPeerMessage
    {
        Success=0,
        AlreadyExists,
        MaximumConnectionsReached
    }
}