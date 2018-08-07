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
        //private ProtoClient<NetworkMessage> seeker; 
        
        /// <summary>Cancellation that is triggered on shutdown to stop all pending operations.</summary>
        private readonly CancellationTokenSource serverCancel;

        public CancellationTokenSource seekReset;
        
        private INetworkConfiguration netConfig;

        private UtilitiesConfiguration utilConf;

        private NetworkPeers Peers;

        private NetworkDiscovery networkDiscovery;

        /// <summary>IP address and port, on which the server listens to incoming connections.</summary>
        public IPEndPoint LocalEndpoint { get; private set; }

        private ConcurrentDictionary<string,ProtoClient<NetworkMessage>> PeerConnections;

        public NetworkSeeker(INetworkConfiguration networkConfiguration, UtilitiesConfiguration utilConf, ref NetworkPeers peers)
        {
            this.serverCancel = new CancellationTokenSource();
            this.seekReset = new CancellationTokenSource();
            this.LocalEndpoint = NetworkDiscovery.GetEndPoint(networkConfiguration.Turf, utilConf, networkConfiguration.ReceivePort);
            this.netConfig = networkConfiguration;
            this.utilConf = utilConf;
            this.Peers = peers;
            this.PeerConnections = new ConcurrentDictionary<string, ProtoClient<NetworkMessage>>();
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
                
                //while (!this.serverCancel.IsCancellationRequested)
                //{                   
                    //await Task.Run(async () =>
                    //{                        
                        //while (!this.seekReset.IsCancellationRequested)
                        //{
                            //Peer discovery begins
                            await networkDiscovery.DiscoverPeersAsync();

                            //Peers populated, let's connect
                            await ConnectAndSaveToPeersAsync();
                            
                            //this.Peers.Clear();
                            await GibGab(this.seekReset);
                        //}
                        //this.seekReset = new CancellationTokenSource();                        
                    //});
                //}
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
                    await ConnectAndSaveToPeersAsync();                    
                    Thread.Sleep(1000);
                }
            });
        }

        private async Task GibGab(CancellationTokenSource seekRst)
        {                          
            await Task.Run(async () =>
            {
                Console.WriteLine("Seeker: starting GibGab");
                while(!seekRst.IsCancellationRequested)
                {
                    foreach(var c in this.PeerConnections)
                    {   
                        NetworkMessage nm = GetLocalMessage(MessageType.Gab);
                        if(c.Value.ConnectionStatus == ConnectionStatus.Connected)
                        {                                                
                            await c.Value.Send(nm);
                        }                                        
                        Thread.Sleep(1000);
                    }
                }
            });
        }        


        ///<summary>Method takes Peers that were discovered and actually connects to them</summary>        
        private async Task ConnectAndSaveToPeersAsync()
        {            
            ProtoClient<NetworkMessage> client;
            var peers = this.Peers.GetPeers();
            //Console.WriteLine("Peer count: {0}", peers.Count);
            foreach (var ePnt in peers)
            {
                if(ePnt.IsConnected) continue;
                IPEndPoint endPoint = ePnt.IPEnd;                
                client = new ProtoClient<NetworkMessage>(endPoint.Address, endPoint.Port) { AutoReconnect = false };
                client.ReceivedMessage += ClientMessageReceivedAsync;                
                this.PeerConnections.TryAdd(endPoint.ToString(),client);
                await client.Connect();
            }
            
        }               

        private void ClientMessageReceivedAsync(IPEndPoint sender, NetworkMessage message)
        {
            try
            {
                Console.WriteLine("Seeker: message ({0}) received", message.MessageStateType.ToString());                
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(message.MessageSenderIP), message.MessageSenderPort);
                if(this.PeerConnections.ContainsKey(endPoint.ToString()))
                {
                    ProtoClient<NetworkMessage> client = this.PeerConnections[endPoint.ToString()];
                    if(message.MessageStateType == MessageType.Accepting)
                    {
                        this.Peers.UpdatePeerConnection(endPoint.ToString(), client);
                        NetworkMessage sndMessage = new NetworkMessage {MessageSenderIP = IPAddress.Loopback.ToString(), MessageSenderPort = netConfig.ReceivePort, MessageStateType = MessageType.Connected, KnownPeers = this.Peers.ConvertPeersToStringArray()};
                        try
                        {                        
                            client.Send(sndMessage);                        
                        }
                        catch(Exception e) 
                        { 
                            Console.WriteLine("Seeker: Socket not responding ({0})", e.ToString()); 
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private NetworkMessage GetLocalMessage(MessageType msg)
        {
            return new NetworkMessage {MessageSenderIP = IPAddress.Loopback.ToString(), 
            MessageSenderPort = netConfig.ReceivePort, 
            MessageStateType = msg,
            KnownPeers = new string[]{""}};
        }             

    }
}
