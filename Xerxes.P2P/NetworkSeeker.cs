using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xerxes.Domain;
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

        private BlockChain BlockChain;

        private NetworkDiscovery networkDiscovery;

        /// <summary>IP address and port, on which the server listens to incoming connections.</summary>
        public IPEndPoint LocalEndpoint { get; private set; }

        private SortedDictionary<DateTime, IPEndPoint> ElderNodes;

        public NetworkSeeker(INetworkConfiguration networkConfiguration, UtilitiesConfiguration utilConf, ref NetworkPeers peers, ref BlockChain blockChain)
        {
            this.serverCancel = new CancellationTokenSource();
            this.seekReset = new CancellationTokenSource();
            this.LocalEndpoint = NetworkDiscovery.GetEndPoint(networkConfiguration.Turf, utilConf, networkConfiguration.ReceivePort);
            this.netConfig = networkConfiguration;
            this.utilConf = utilConf;
            this.Peers = peers;
            this.BlockChain = blockChain;
            this.networkDiscovery = new NetworkDiscovery(this.netConfig, peers, this.utilConf);
            this.ElderNodes = new SortedDictionary<DateTime, IPEndPoint>();
        }

        public async Task SeekPeersAsync()
        {
            try
            {
                int delay = this.utilConf.GetOrDefault<int>("peerdiscoveryin", 86400000);
                UtilitiesLogger.WriteLine(LoggerType.Debug, "Seeker: Seeking Peers");
                await networkDiscovery.DiscoverPeersAsync();
                UtilitiesLogger.WriteLine(LoggerType.Debug, "Seeker: {0} Peers Discovered, attempting to connect", this.Peers.GetPeerCount());
                while (!this.serverCancel.IsCancellationRequested)
                { 
                    await ConnectToPeers();
                    Thread.Sleep(1000);
                }
            }
            catch(Exception ex) { UtilitiesLogger.WriteLine(LoggerType.Error, ex.ToString()); }
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
                        UtilitiesLogger.WriteLine(LoggerType.Info, "Seeker: created a new socket for {0}", p.IPEnd.ToString());
                        protoClient.ReceivedMessage += ClientReceivedMessage;
                        protoClient.ConnectionLost += ProtoClient_ConnectionLost;                        
                        await p.ProtoClient.Connect(true);
                        if(p.ProtoClient.ConnectionStatus == ConnectionStatus.Connected)
                        {
                            //lets send back a status of connected
                            p.IsConnected = true;
                            NetworkMessage networkMessage = new NetworkMessage { MessageSenderIP = this.LocalEndpoint.Address.ToString(), MessageSenderPort = this.netConfig.ReceivePort, MessageStateType = MessageType.Connected, KnownPeers = this.Peers.ConvertPeersToStringArray() };
                            Thread.Sleep(1000);
                            await p.ProtoClient.Send(networkMessage);

                            //if our blockchain has only a single block, we're not up-to-date, let's request ages from the nodes
                            if(this.BlockChain.Count() == 1)
                            {
                                networkMessage.MessageStateType = MessageType.RequestAge;
                                await p.ProtoClient.Send(networkMessage);
                            }
                        }
                    }
                    catch(Exception e)
                    {                        
                        UtilitiesLogger.WriteLine(LoggerType.Error, "Seeker: error while connecting ({0})", e.ToString());
                    }
                }
            }

            if (this.ElderNodes.Count > 0 && this.BlockChain.Count() == 1)
            {
                ProtoClient<NetworkMessage> client;
                NetworkMessage message = new NetworkMessage { MessageSenderIP = this.LocalEndpoint.Address.ToString(), MessageSenderPort = this.netConfig.ReceivePort, MessageStateType = MessageType.DownloadChain };
                if (this.Peers.GetPeer(this.ElderNodes.First().Value.ToString()) != null)
                {
                    client = this.Peers.GetPeer(this.ElderNodes.First().Value.ToString()).ProtoClient;
                    await client.Send(message);
                }
            }

        }

        private void ProtoClient_ConnectionLost(IPEndPoint endPoint)
        {
            if (this.Peers.GetPeer(endPoint.ToString()) != null)
            {
                this.Peers.GetPeer(endPoint.ToString()).ProtoClient.AutoReconnect = false;
                this.Peers.RemovePeer(endPoint);
                UtilitiesLogger.WriteLine(LoggerType.Info, "Seeker: destroyed socket on {0}", endPoint.ToString());
            }
        }

        private void ClientReceivedMessage(IPEndPoint senderEndPoint, NetworkMessage message)
        {
            if(message.MessageStateType == MessageType.RequestAge)
            {
                //UtilitiesLogger.WriteLine(LoggerType.Info, "Seeker: I found a node with age {0}", message.Age);
                this.ElderNodes.TryAdd(message.Age, senderEndPoint);
            }

            if (message.MessageStateType == MessageType.DownloadChain && message.Age == this.ElderNodes.First().Key)
            {
                BlockChain sentChain = message.BlockChain;
                if(sentChain.Count() > 1 && this.BlockChain.Count() == 1)
                {
                    this.BlockChain = sentChain;
                    UtilitiesLogger.WriteLine(LoggerType.Info, "Seeker: Downloaded block [{0}]", BlockChain.PrintChain());
                }
            }
        }
    }
}
