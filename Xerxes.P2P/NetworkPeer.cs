using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Xerxes.Utils;

namespace Xerxes.P2P
{    
    public class NetworkPeer:INetworkPeer
    {
        public IPEndPoint IPEnd{ get;set;}
        public string Id {get;set;}
        public TcpClient TCPClient { get;set; }
        public bool IsConnected {get;set;}

        public NetworkPeer(IPEndPoint ipEnd)
        {
            this.IPEnd = ipEnd;
        }

        public async void SendMessage(string response, NetworkStream receiver)
        {
            byte[] serverResponseBytes = Encoding.UTF8.GetBytes(response);
            await receiver.WriteAsync(serverResponseBytes, 0, serverResponseBytes.Length);
        }

        public override string ToString()
        {
            return this.IPEnd.ToString();
        }
    }

    public class NetworkPeers
    {
        private ConcurrentDictionary<string, NetworkPeer> peers;
        private int maxinbound{get;set;}
        private int maxoutbound{get;set;}

        public int GetPeerCount(IPEndPoint owner)
        {
            return (this.peers.ContainsKey(owner.ToString())) ? this.peers.Count - 1 : this.peers.Count;
        }

        public NetworkPeers(int maxinbound, int maxoutbound)
        {
            this.peers = new ConcurrentDictionary<string, NetworkPeer>();
            this.maxinbound = maxinbound;
            this.maxoutbound = maxoutbound;
        }        

  
        public void Clear()
        {
            this.peers.Clear();
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

        public NetworkPeerMessage AddOutboundPeer(NetworkPeer toBeAdded)
        {
            if (this.peers.Count < maxoutbound)
            {
                bool result = this.peers.TryAdd(toBeAdded.IPEnd.ToString(), toBeAdded);
                if (result)
                    return NetworkPeerMessage.Success;
                else
                    return NetworkPeerMessage.AlreadyExists;
            }
            else
            {
                return NetworkPeerMessage.MaximumConnectionsReached;
            }
        }                      

        public List<NetworkPeer> GetPeers()
        {
            return this.peers.Values.ToList();
        }

        public void CombinePeers(string[] knownPeers)
        {
            if(knownPeers.Length > 0)
            {
                foreach (string kp in knownPeers)
                {
                    try
                    {
                        IPEndPoint converted = UtilitiesNetwork.CreateIPEndPoint(kp);
                        try
                        {
                            NetworkPeerMessage res = AddOutboundPeer(new NetworkPeer(converted));
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }

                    }
                    catch
                    {
                        Console.WriteLine("ERROR While Combining Peers");
                    }
                }
            }            
        }

        public string[] ConvertPeersToStringArray()
        {
            return this.peers.Keys.ToArray();    
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if(this.peers.Count > 0)
            {
                foreach(var p in peers)
                {   
                    string output = (p.Value.IPEnd!=null) ? p.Value.IPEnd.ToString() : "";
                    sb.Append(output+ " ");
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