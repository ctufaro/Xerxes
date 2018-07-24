using System;
using System.Net;
using System.Collections.Generic;
using Xerxes.Utils;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Linq;

namespace Xerxes.P2P
{
    /// <summary>    
    /// When a user first starts receiving, they need to connect to seed nodes
    /// When a user is seeking, they need to be notified when a new peer has joined the network and update
    /// their peer list.
    /// </summary>
    public class NetworkDiscovery
    {
        private INetworkConfiguration networkConfiguration;
        private UtilitiesConfiguration utilConf;
        private List<IPEndPoint> endPoints;

        public NetworkDiscovery(INetworkConfiguration networkConfiguration, List<IPEndPoint> endPoints, UtilitiesConfiguration utilConf)
        {
            this.networkConfiguration = networkConfiguration;
            this.utilConf = utilConf;
            this.endPoints = endPoints;
        }

        public async Task DiscoverPeersAsync()
        {
            await Task.Run(async () =>
            {                    
                await GetConnectionsFromDBAsync();
                await GetConnectionsFromSeedsAsync();
            });  
        }

        private async Task GetConnectionsFromDBAsync()
        {   
            await Task.Run(()=>
            {
                string pathToDatabase = UtilitiesGeneral.GetApplicationRoot("Xerxes.db");
                Merge(UtilitiesDatabase.GetSavedConnectionsFromDB(pathToDatabase));            
            });
        }

        private async Task GetConnectionsFromSeedsAsync()
        {
            await Task.Run(()=>
            {
                if(networkConfiguration.Turf == Turf.Intranet)
                {
                    string[] ports = utilConf.GetOrDefault<string>("intraports","").Split(',', StringSplitOptions.RemoveEmptyEntries);
                    IEnumerable<IPEndPoint> ipList = UtilitiesNetwork.GetRandomSeedPorts(ports, utilConf.GetOrDefault<int>("seedsreturned",3));
                    Merge(ipList);
                }
                else if(networkConfiguration.Turf == Turf.TestNet)
                {
                    int port = utilConf.GetOrDefault<int>("testnet",0);
                    string[] dnsnames = utilConf.GetOrDefault<string>("dnsseeds","").Split(',', StringSplitOptions.RemoveEmptyEntries);
                    IEnumerable<IPAddress> ipList = UtilitiesNetwork.GetRandomSeedNodes(dnsnames, utilConf.GetOrDefault<int>("seedsreturned",3));
                    Merge(ipList, port);
                }
                else if(networkConfiguration.Turf == Turf.MainNet)
                {
                    int port = utilConf.GetOrDefault<int>("mainnet",0);
                    string[] dnsnames = utilConf.GetOrDefault<string>("dnsseeds","").Split(',', StringSplitOptions.RemoveEmptyEntries);
                    IEnumerable<IPAddress> ipList = UtilitiesNetwork.GetRandomSeedNodes(dnsnames, utilConf.GetOrDefault<int>("seedsreturned",3));
                    Merge(ipList, port);
                }
            });            
        }

        private void Merge(IEnumerable<IPEndPoint> ipList)
        {
            List<IPEndPoint> toBeMerged = ipList.ToList();
            if(toBeMerged.Count > 0)
            {
                foreach(IPEndPoint iEP in toBeMerged)
                {
                    this.endPoints.Add(iEP);
                }
            }
        }

        private void Merge(IEnumerable<IPAddress> ipList, int port)
        {
            List<IPAddress> toBeMerged = ipList.ToList();
            if(toBeMerged.Count > 0)
            {
                foreach(IPAddress iAD in toBeMerged)
                {
                    IPEndPoint iEP = new IPEndPoint(iAD, port);
                    this.endPoints.Add(iEP);
                }
            }
        }

        public static IPEndPoint GetEndPoint(Turf turf, UtilitiesConfiguration utilConf, int intranetPort)
        {
            IPAddress externalIP = null;
            int? port = null;

            if (turf == Turf.Intranet)
            {
                externalIP = IPAddress.Loopback;
                port = intranetPort;
            }

            else if (turf == Turf.TestNet)
            {
                externalIP = UtilitiesNetwork.GetMyIPAddress();
                port = utilConf.GetOrDefault<int>("testnet", 0);
            }

            else if (turf == Turf.MainNet)
            {
                externalIP = UtilitiesNetwork.GetMyIPAddress();
                port = utilConf.GetOrDefault<int>("mainnet", 0);
            }

            return new IPEndPoint(externalIP, port.Value);
        }

    }
}