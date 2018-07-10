using System;
using System.Net;
using System.Collections.Generic;
using Xerxes.Utils;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;

namespace Xerxes.P2P
{
    /// <summary>    
    /// When a user first starts receiving, they need to broadcast their existence to street nodes
    /// When a user is seeking, they need to be notified when a new peer has joined the network and update
    /// their peer list.
    /// </summary>
    public class NetworkDiscovery
    {
        private readonly CancellationTokenSource serverCancel;
        private INetworkConfiguration networkConfiguration;
        private UtilitiesConfiguration utilConf;
        private NetworkPeers peers;
        public NetworkDiscovery(INetworkConfiguration networkConfiguration, NetworkPeers peers, UtilitiesConfiguration utilConf)
        {
            this.networkConfiguration = networkConfiguration;
            this.utilConf = utilConf;
            this.serverCancel = new CancellationTokenSource();
            this.peers = peers;
        }

        public async Task<List<IPEndPoint>> DiscoverPeersAsync(CancellationTokenSource seekReset)
        {
            while (!seekReset.IsCancellationRequested)  
            {                   
                await Task.Run(async () =>
                {                    
                    GetConnectionsFromDB();
                    GetConnectionsFromStreet();
                    await BroadcastSeekAsync();
                    seekReset.Cancel();
                });                    
            }
            return null;    
        }

        private void GetConnectionsFromDB()
        {
            string pathToDatabase = UtilitiesGeneral.GetApplicationRoot("Xerxes.db");
            Merge(UtilitiesDatabase.GetSavedConnectionsFromDB(pathToDatabase));            
        }

        private void GetConnectionsFromStreet()
        {
            if(networkConfiguration.Turf == Turf.Intranet)
            {
                string[] ports = utilConf.GetOrDefault<string>("intraports","").Split(',', StringSplitOptions.RemoveEmptyEntries);
                Merge(UtilitiesNetwork.GetStreetPorts(ports));
            }
            else if(networkConfiguration.Turf == Turf.TestNet)
            {
                int port = utilConf.GetOrDefault<int>("testnet",0);
                string[] dnsnames = utilConf.GetOrDefault<string>("dnsseeds","").Split(',', StringSplitOptions.RemoveEmptyEntries);
                Merge(UtilitiesNetwork.GetStreetNodes(dnsnames), port);
            }
            else if(networkConfiguration.Turf == Turf.MainNet)
            {
                int port = utilConf.GetOrDefault<int>("mainnet",0);
                string[] dnsnames = utilConf.GetOrDefault<string>("dnsseeds","").Split(',', StringSplitOptions.RemoveEmptyEntries);
                Merge(UtilitiesNetwork.GetStreetNodes(dnsnames), port);
            }            
        }

        private async Task BroadcastSeekAsync()
        {            
            foreach(NetworkPeer peer in peers.peers.Values)
            {             
                Console.WriteLine("Broadcasting to Peer: {0}", peer.IPEnd.ToString());
                using (var tcpClient = new TcpClient())
                {
                    await tcpClient.ConnectAsync(peer.IPEnd.Address, peer.IPEnd.Port);
                    System.Console.WriteLine("Connected to peer, sending seek message..");
                    NetworkMessage nm = new NetworkMessage();
                    IPEndPoint myEndPoint = GetMyEndPoint();
                    nm.MessageSenderIP = myEndPoint.Address.ToString();
                    nm.MessageSenderPort = myEndPoint.Port;
                    nm.MessageStateType = NetworkStateType.Seek;
                    string json = NetworkMessage.NetworkMessageToJSON(nm);
                    byte[] bytes = Encoding.UTF8.GetBytes(json);
                    using (var networkStream = tcpClient.GetStream())
                    {
                        Console.WriteLine("Sending to Peer {0}", json);
                        await networkStream.WriteAsync(bytes, 0, bytes.Length);
                    }                  

                }                
            }
        }

        private IPEndPoint GetMyEndPoint()
        {
            if(networkConfiguration.Turf == Turf.Intranet)
            {
                int port = this.networkConfiguration.ReceivePort;
                return new IPEndPoint(IPAddress.Loopback, port); 
            }
            else if(networkConfiguration.Turf == Turf.TestNet)
            {
                int port = utilConf.GetOrDefault<int>("testnet",0);
                return new IPEndPoint(UtilitiesNetwork.GetMyIPAddress(), port); 
            }
            else 
            {
                int port = utilConf.GetOrDefault<int>("mainnet",0);
                return new IPEndPoint(UtilitiesNetwork.GetMyIPAddress(), port);
            } 
        }

        private void Merge(List<IPEndPoint> toBeMerged)
        {
            if(toBeMerged.Count > 0)
            {
                foreach(IPEndPoint iEP in toBeMerged)
                {
                    this.peers.AddPeer(new NetworkPeer(iEP));
                }
            }
        }
        private void Merge(List<IPAddress> toBeMerged, int port)
        {
            if(toBeMerged.Count > 0)
            {
                foreach(IPAddress iAD in toBeMerged)
                {
                    IPEndPoint iEP = new IPEndPoint(iAD, port);
                    this.peers.AddPeer(new NetworkPeer(iEP));
                }
            }
        }

    }
}