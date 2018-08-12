using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using GenericProtocol.Implementation;
using Xerxes.Utils;

namespace Xerxes.P2P
{    
    public class NetworkPeer:INetworkPeer
    {
        public IPEndPoint IPEnd{ get;set;}
        public string Id {get;set;}
        public ProtoClient<NetworkMessage> ProtoClient {get;set;}
        public bool IsConnected {get;set;}

        public NetworkPeer(IPEndPoint ipEnd)
        {
            this.IPEnd = ipEnd;
            this.IsConnected = false;
        }

        public override string ToString()
        {
            return this.IPEnd.ToString();
        }
    }

    public class NetworkPeers
    {
        private ConcurrentDictionary<string, NetworkPeer> peers;
        public int MaxInBound{get;}
        public int MaxOutBound{get;}

        public int GetPeerCount()
        {
            return this.peers.Count;
        }

        public NetworkPeers(int maxinbound, int maxoutbound)
        {
            this.peers = new ConcurrentDictionary<string, NetworkPeer>();
            this.MaxInBound = maxinbound;
            this.MaxOutBound = maxoutbound;
        }        

  
        public void Clear()
        {
            this.peers.Clear();
        }

        public NetworkPeerMessage AddInboundPeer(NetworkPeer toBeAdded)
        {
            if(this.peers.Count < MaxInBound)
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
            if (this.peers.Count < MaxOutBound)
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

        public NetworkPeer GetPeer(string ipEndPoint)
        {
            return this.peers[ipEndPoint];
        }

        public void UpdatePeerConnection(string peerId, ProtoClient<NetworkMessage> proto)
        {
            NetworkPeer peer = this.peers[peerId];
            peer.ProtoClient = proto;
            peer.IsConnected = true;
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
                        UtilitiesLogger.WriteLine("ERROR While Combining Peers", LoggerType.Error);
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
        MaximumConnectionsReached,
        Error
    }
}