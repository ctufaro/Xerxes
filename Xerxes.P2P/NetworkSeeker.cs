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
                            
                            this.peers.Clear();
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
            Console.WriteLine("From the seeker: Peers to Connect to {0}", this.foundEndPoints.Count);
            Thread.Sleep(5000);
            foreach (var endPoint in this.foundEndPoints)
            {
                IPEndPoint myLocalEnd = NetworkDiscovery.GetEndPoint(netConfig.Turf, utilConf, netConfig.ReceivePort);
                this.seeker = new ProtoClient<NetworkMessage>(endPoint.Address, endPoint.Port) { AutoReconnect = true };
                this.seeker.ReceivedMessage += ClientMessageReceived;
                await this.seeker.Connect(true);
                Console.WriteLine("From the seeker: Connecting to " + endPoint.ToString());
                NetworkMessage sndMessage = new NetworkMessage { MessageSenderIP = endPoint.Address.ToString(), MessageSenderPort = endPoint.Port, MessageStateType = MessageType.Seek };
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
