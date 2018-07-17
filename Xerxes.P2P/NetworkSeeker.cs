using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xerxes.Utils;

namespace Xerxes.P2P
{
    public class NetworkSeeker
    {
        /// <summary>Cancellation that is triggered on shutdown to stop all pending operations.</summary>
        private readonly CancellationTokenSource serverCancel;

        public CancellationTokenSource seekReset;
        
        private INetworkConfiguration networkConfiguration;

        private UtilitiesConfiguration utilConf;

        private NetworkPeers foundPeers;
        private NetworkPeers establishedPeers;
        private NetworkDiscovery networkDiscovery;

        private NetworkPeerConnection networkPeerConnection;

        public NetworkSeeker(INetworkConfiguration networkConfiguration, UtilitiesConfiguration utilConf)
        {
            this.serverCancel = new CancellationTokenSource();
            this.seekReset = new CancellationTokenSource();
            this.networkConfiguration = networkConfiguration;
            this.utilConf = utilConf;
            this.foundPeers = new NetworkPeers();
            this.establishedPeers = new NetworkPeers();
            this.networkDiscovery = new NetworkDiscovery(this.networkConfiguration, this.foundPeers, this.utilConf);
            this.networkPeerConnection = new NetworkPeerConnection(this.networkConfiguration, this.foundPeers, this.establishedPeers, this.utilConf);
        }

        public async void SeekPeersAsync()
        {
            try
            {              
                int delay = this.utilConf.GetOrDefault<int>("peerdiscoveryin",86400000);
                UtilitiesConsole.Update(UCommand.Status, "Seeking Peers");
                Thread.Sleep(1000);
                
                while (!this.serverCancel.IsCancellationRequested)
                {                   
                    await Task.Run(async () =>
                    {                        
                        while (!this.seekReset.IsCancellationRequested)
                        {
                            //Peer discovery begins
                            await networkDiscovery.DiscoverPeersAsync();
                            //Console.WriteLine(this.peers);

                            //Peers populated, let's attempt to connect
                            await AttemptToConnectAsync(delay,this.seekReset);
                            
                            this.establishedPeers.Clear();
                        }
                        this.seekReset = new CancellationTokenSource();                        
                    });
                }
            }
            catch { }
        }

        public async Task AttemptToConnectAsync(int delay, CancellationTokenSource seekRst)
        {
            await Task.Run(async ()=>
            {
                //Console.WriteLine("Attempting to connecting to peers"); 
                seekRst.CancelAfter(delay);
                while(!seekRst.IsCancellationRequested)
                {                    
                    await ConnectToPeersAsync();                    
                    Thread.Sleep(1000);
                }
            });
        }

        public async Task ConnectToPeersAsync()
        {
            foreach(var peer in this.foundPeers.peers)
            {
                await networkPeerConnection.BroadcastSingleSeekAsync(peer.Value);
            }            
        }               

    }
}
