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

        private NetworkPeers peers;        

        public NetworkSeeker(INetworkConfiguration networkConfiguration, UtilitiesConfiguration utilConf)
        {
            this.serverCancel = new CancellationTokenSource();
            this.seekReset = new CancellationTokenSource();
            this.networkConfiguration = networkConfiguration;
            this.utilConf = utilConf;
            this.peers = new NetworkPeers();
        }

        public async void SeekPeersAsync()
        {
            try
            {                
                NetworkDiscovery networkDiscovery = new NetworkDiscovery(this.networkConfiguration, this.peers, this.utilConf);
                int delay = this.utilConf.GetOrDefault<int>("peerdiscoveryin",86400000);
                Console.WriteLine("Seeking peers in {0} seconds", delay/1000);
                
                while (!this.serverCancel.IsCancellationRequested)
                {                   
                    await Task.Run(async () =>
                    {                        
                        while (!this.seekReset.IsCancellationRequested)
                        {
                            //Peer discovery begins
                            await networkDiscovery.DiscoverPeersAsync(this.seekReset);
                            Console.WriteLine(this.peers);
                        }
                        
                        //Peer discovery stopped
                        this.seekReset = new CancellationTokenSource();
                    });

                    //Peers populated, let's attempt to connect

                    //will resume discovery in 
                    await Task.Delay(delay, this.serverCancel.Token);
                }
            }
            catch { }
        } 

               

    }
}
