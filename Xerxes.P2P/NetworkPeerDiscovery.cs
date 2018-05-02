using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Xerxes.P2P
{
    ///https://en.bitcoin.it/wiki/Satoshi_Client_Node_Discovery
    public class NetworkPeerDiscovery
    {
        List<NetworkPeer> connectedPeers = new List<NetworkPeer>();
        
        public NetworkPeerDiscovery(List<INetworkPeer> trustedPeers)
        {            
            foreach(NetworkPeer n in trustedPeers)
                connectedPeers.Add(n);
        }
    }
}