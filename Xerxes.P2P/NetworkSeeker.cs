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
        private ProtoClient<NetworkMessage> seeker; 
        
        /// <summary>Cancellation that is triggered on shutdown to stop all pending operations.</summary>
        private readonly CancellationTokenSource serverCancel;

        public CancellationTokenSource seekReset;
        
        private INetworkConfiguration netConfig;

        private UtilitiesConfiguration utilConf;

        private NetworkPeers Peers;

        private NetworkDiscovery networkDiscovery;

        /// <summary>IP address and port, on which the server listens to incoming connections.</summary>
        public IPEndPoint LocalEndpoint { get; private set; }

        public NetworkSeeker(INetworkConfiguration networkConfiguration, UtilitiesConfiguration utilConf, ref NetworkPeers peers)
        {
            this.serverCancel = new CancellationTokenSource();
            this.seekReset = new CancellationTokenSource();
            this.LocalEndpoint = NetworkDiscovery.GetEndPoint(networkConfiguration.Turf, utilConf, networkConfiguration.ReceivePort);
            this.netConfig = networkConfiguration;
            this.utilConf = utilConf;
            this.Peers = peers;
            this.networkDiscovery = new NetworkDiscovery(this.netConfig, peers, this.utilConf);            
        }

        public async void SeekPeersAsync()
        {
            try
            {              
                int delay = this.utilConf.GetOrDefault<int>("peerdiscoveryin",86400000);
                //UtilitiesConsole.Update(UCommand.StatusOutbound, "Seeking Peers");
                Console.WriteLine("From the seeker: Seeking Peers");
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
                            
                            this.Peers.Clear();
                        }
                        this.seekReset = new CancellationTokenSource();                        
                    });
                }
            }
            catch(Exception ex) { Console.WriteLine(ex.ToString()); }
        }

        private async Task AttemptToConnectAsync(int delay, CancellationTokenSource seekRst)
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

        private async Task ConnectToPeersAsync()
        {
            //UtilitiesConsole.Update(UCommand.StatusOutbound, "Peers to Connect to: " + this.foundEndPoints.Count);
            var peersToSeek = this.Peers.GetPeers();
            Console.WriteLine("From the seeker: Peers to Connect to {0}", this.Peers.GetPeerCount(LocalEndpoint));
            Thread.Sleep(5000);
            foreach (var ePnt in peersToSeek)
            {
                IPEndPoint endPoint = ePnt.IPEnd;
                IPEndPoint myLocalEnd = NetworkDiscovery.GetEndPoint(netConfig.Turf, utilConf, netConfig.ReceivePort);
                this.seeker = new ProtoClient<NetworkMessage>(endPoint.Address, endPoint.Port) { AutoReconnect = true };
                this.seeker.ReceivedMessage += ClientMessageReceived;
                await this.seeker.Connect(true);
                Console.WriteLine("From the seeker: Connecting to " + endPoint.ToString());
                NetworkMessage sndMessage = new NetworkMessage
                {
                    MessageSenderIP = IPAddress.Loopback.ToString(),
                    MessageSenderPort = netConfig.ReceivePort,
                    MessageStateType = MessageType.Seek,
                    KnownPeers = this.Peers.ConvertPeersToStringArray()
                };
                try
                {
                    await this.seeker.Send(sndMessage);
                }
                catch { Console.WriteLine("Socket not responding"); }
            }            
        }               

        private void ClientMessageReceived(IPEndPoint sender, NetworkMessage message)
        {
            Console.WriteLine($"All is good! {message.MessageSenderIP}:{message.MessageSenderPort} {message.MessageStateType}");
        }             

    }
}
