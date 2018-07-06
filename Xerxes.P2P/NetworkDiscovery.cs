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
        private ConcurrentBag<IPEndPoint> peers;
        public NetworkDiscovery(INetworkConfiguration networkConfiguration, ConcurrentBag<IPEndPoint> peers)
        {
            this.networkConfiguration = networkConfiguration;
            this.serverCancel = new CancellationTokenSource();
            this.peers = peers;
        }

        public async Task<List<IPEndPoint>> DiscoverPeersAsync(CancellationTokenSource seekReset)
        {
            while (!seekReset.IsCancellationRequested)  
            {                   
                await Task.Run(() =>
                {
                    Console.WriteLine("In DiscoveryPeers Method");
                    Thread.Sleep(1000);
                    this.peers.Add(new IPEndPoint(0,0));
                    Console.WriteLine("In DiscoveryPeers Method, calling cancel");
                    seekReset.Cancel();
                });                    
            }
            return null;    
        }

        private List<IPEndPoint> GetSavedConnections()
        {
            //TODO: Implement this
            List<IPEndPoint> ipEndPoints = new List<IPEndPoint>();
            return ipEndPoints;
        }

    }
}