using System;
using System.Net;
using System.Collections.Generic;
using Xerxes.Utils;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

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
                await Task.Run(() =>
                {                    
                    GetConnectionsFromDB();
                    GetConnectionsFromStreet();                    
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