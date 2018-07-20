using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GenericProtocol.Implementation;
using Xerxes.Utils;

namespace Xerxes.P2P
{
    public class NetworkSeeker
    {
        private ProtoClient<string> seeker; 
        /// <summary>Cancellation that is triggered on shutdown to stop all pending operations.</summary>
        private readonly CancellationTokenSource serverCancel;

        public CancellationTokenSource seekReset;
        
        private INetworkConfiguration netConfig;

        private UtilitiesConfiguration utilConf;

        private List<IPEndPoint> foundEndPoints;
        private NetworkPeers peers;
        private NetworkDiscovery networkDiscovery;

        public NetworkSeeker(INetworkConfiguration networkConfiguration, UtilitiesConfiguration utilConf)
        {
            this.serverCancel = new CancellationTokenSource();
            this.seekReset = new CancellationTokenSource();
            this.netConfig = networkConfiguration;
            this.utilConf = utilConf;
            this.foundEndPoints = new List<IPEndPoint>();
            this.peers = new NetworkPeers();
            this.networkDiscovery = new NetworkDiscovery(this.netConfig, this.foundEndPoints, this.utilConf);            
        }

        public async void SeekPeersAsync()
        {
            try
            {              
                int delay = this.utilConf.GetOrDefault<int>("peerdiscoveryin",86400000);
                UtilitiesConsole.Update(UCommand.StatusOutbound, "Seeking Peers");
                Thread.Sleep(1000);
                
                while (!this.serverCancel.IsCancellationRequested)
                {                   
                    await Task.Run(async () =>
                    {                        
                        while (!this.seekReset.IsCancellationRequested)
                        {
                            //Peer discovery begins
                            await networkDiscovery.DiscoverPeersAsync();

                            //Peers populated, let's attempt to connect
                            await AttemptToConnectAsync(delay,this.seekReset);
                            
                            this.peers.Clear();
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
            UtilitiesConsole.Update(UCommand.StatusOutbound, "Peers to Connect to: " + this.foundEndPoints.Count);
            foreach(var endPoint in this.foundEndPoints)
            {
                IPEndPoint myLocalEnd = GetEndPoint(netConfig.Turf, utilConf, netConfig.ReceivePort);
                this.seeker = new ProtoClient<string>(endPoint.Address, endPoint.Port) { AutoReconnect = false };
                this.seeker.ReceivedMessage += ClientMessageReceived;
                await this.seeker.Connect(true);                
                UtilitiesConsole.Update(UCommand.StatusOutbound, "Sending Seek To " + endPoint.ToString());
                await this.seeker.Send("0");                
            }            
        }               

        private void ClientMessageReceived(IPEndPoint sender, string message)
        {
            Console.WriteLine($"All is good! {sender}: {message}");
        }

        public IPEndPoint GetEndPoint(Turf turf, UtilitiesConfiguration utilConf, int intranetPort)
        {
            IPAddress externalIP = null;
            int? port = null;

            if(turf == Turf.Intranet)
            {
                externalIP = IPAddress.Loopback;
                port = intranetPort;
            }

            else if(turf == Turf.TestNet)
            {
                externalIP = UtilitiesNetwork.GetMyIPAddress();
                port = utilConf.GetOrDefault<int>("testnet",0);                
            }

            else if(turf == Turf.MainNet)
            {
                externalIP = UtilitiesNetwork.GetMyIPAddress();
                port = utilConf.GetOrDefault<int>("mainnet",0);
            }

            return new IPEndPoint(externalIP, port.Value);
        }        

    }
}
