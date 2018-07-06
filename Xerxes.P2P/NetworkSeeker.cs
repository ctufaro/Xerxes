using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Xerxes.P2P
{
    public class NetworkSeeker
    {
        /// <summary>Cancellation that is triggered on shutdown to stop all pending operations.</summary>
        private readonly CancellationTokenSource serverCancel;

        public CancellationTokenSource seekReset;
        
        private INetworkConfiguration networkConfiguration;

        private ConcurrentBag<IPEndPoint> peers;

        public NetworkSeeker(INetworkConfiguration networkConfiguration)
        {
            this.serverCancel = new CancellationTokenSource();
            this.seekReset = new CancellationTokenSource();
            this.networkConfiguration = networkConfiguration;
            this.peers = new ConcurrentBag<IPEndPoint>();
        }

        public async void SeekPeersAsync()
        {
            try
            {
                Console.WriteLine("Seeking peers");
                NetworkDiscovery networkDiscovery = new NetworkDiscovery(this.networkConfiguration, this.peers);
                
                while (!this.serverCancel.IsCancellationRequested)
                {                   
                    await Task.Run(async () =>
                    {                        
                        while (!this.seekReset.IsCancellationRequested)
                        {
                            Thread.Sleep(1000);
                            Console.WriteLine("About to Discover Peers");
                            await networkDiscovery.DiscoverPeersAsync(this.seekReset);                            
                        }
                        Console.WriteLine("Discovery Stopped, looping to top again, peer count:{0}", this.peers.Count);
                        this.seekReset = new CancellationTokenSource();
                    });                    
                }
            }
            catch { }
        } 

               

    }
}
