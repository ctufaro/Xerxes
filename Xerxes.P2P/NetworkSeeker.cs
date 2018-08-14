using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xerxes.TCP.Implementation;
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

        public bool TurnColor = false;

        public async Task SeekPeersAsync()
        {
            try
            {
                int delay = this.utilConf.GetOrDefault<int>("peerdiscoveryin", 86400000);
                UtilitiesLogger.WriteLine("Seeker: Seeking Peers", LoggerType.Debug);
                await networkDiscovery.DiscoverPeersAsync();
                UtilitiesLogger.WriteLine(string.Format("Seeker: {0} Peers Discovered, attempting to connect", this.Peers.GetPeerCount()), LoggerType.Debug);
                while (!this.serverCancel.IsCancellationRequested)
                { 
                    await ConnectToPeers();
                    Thread.Sleep(1000);
                }
            }
            catch(Exception ex) { UtilitiesLogger.WriteLine(ex.ToString(), LoggerType.Error); }
        }

        private async Task ConnectToPeers()
        {
            //iterate through the peers discovered
            foreach(NetworkPeer p in this.Peers.GetPeers())
            {
                //lets try to connect and save the socket (not ours)
                if (!p.IsConnected && p.IPEnd.Port != this.netConfig.ReceivePort)
                {
                    try
                    {
                        //lets create a new socket for each peer and save it
                        ProtoClient<NetworkMessage> protoClient = new ProtoClient<NetworkMessage>(p.IPEnd.Address, p.IPEnd.Port);
                        p.ProtoClient = protoClient;
                        p.ProtoClient.AutoReconnect = true;
                        UtilitiesLogger.WriteLine(string.Format("Seeker: created a new socket for {0}", p.IPEnd.ToString()), LoggerType.Info);
                        protoClient.ReceivedMessage += ClientReceivedMessage;
                        protoClient.ConnectionLost += ProtoClient_ConnectionLost;
                        await p.ProtoClient.Connect(true);
                        if(p.ProtoClient.ConnectionStatus == ConnectionStatus.Connected)
                        {
                            //lets send a status of connected
                            p.IsConnected = true;
                            NetworkMessage networkMessage = new NetworkMessage { MessageSenderIP = this.LocalEndpoint.Address.ToString(), MessageSenderPort = this.netConfig.ReceivePort, MessageStateType = MessageType.Connected, KnownPeers = this.Peers.ConvertPeersToStringArray() };
                            await p.ProtoClient.Send(networkMessage);
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Seeker: error while connecting ({0})", e.ToString());
                    }
                }
            }           

        }

        private void ProtoClient_ConnectionLost(IPEndPoint endPoint)
        {
            this.Peers.GetPeer(endPoint.ToString()).ProtoClient.AutoReconnect = false;
            this.Peers.RemovePeer(endPoint);
            UtilitiesLogger.WriteLine(string.Format("Seeker: destroyed socket on {0}", endPoint.ToString()), LoggerType.Info);

        }

        private void ClientReceivedMessage(IPEndPoint senderEndPoint, NetworkMessage message)
        {
            
        }
    }
}
