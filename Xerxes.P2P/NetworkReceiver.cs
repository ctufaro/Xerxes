using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xerxes.Utils;
using Xerxes.TCP;
using Xerxes.TCP.Implementation;
using System.Linq;
using Xerxes.Domain;

namespace Xerxes.P2P
{
    public class NetworkReceiver
    {
        /// <summary>Cancellation that is triggered on shutdown to stop all pending operations.</summary>
        private readonly CancellationTokenSource serverCancel;

        private INetworkConfiguration networkConfiguration;

        private UtilitiesConfiguration utilConf;                

        /// <summary>TCP server listener accepting inbound connections.</summary>
        private ProtoServer<NetworkMessage> receiver;     

        /// <summary>IP address and port, on which the server listens to incoming connections.</summary>
        public IPEndPoint LocalEndpoint { get; private set; }
        
        /// <summary>List of all inbound peers.</summary>
        private NetworkPeers Peers;

        private BlockChain BlockChain;
       
        public NetworkReceiver(INetworkConfiguration netConfig, UtilitiesConfiguration utilConf, ref NetworkPeers peers, ref BlockChain blockChain)
        {
            this.utilConf = utilConf;
            this.LocalEndpoint = NetworkDiscovery.GetEndPoint(netConfig.Turf, utilConf, netConfig.ReceivePort);
            this.receiver = new ProtoServer<NetworkMessage>(this.LocalEndpoint.Address, this.LocalEndpoint.Port);
            this.serverCancel = new CancellationTokenSource();
            this.networkConfiguration = netConfig;
            this.Peers = peers;
            this.BlockChain = blockChain;
        }

        public async Task ReceivePeersAsync()    
        {
            try
            {
                await Task.Run(() =>
                {                    
                    receiver.Start();
                    UtilitiesLogger.WriteLine(LoggerType.Info, "Receiver: Server started on {0}", this.LocalEndpoint.ToString());
                    receiver.ClientConnected += ClientConnectedAsync;
                    receiver.ReceivedMessage += ServerMessageReceivedAsync;
                });
            }
            catch (Exception e)
            {
                UtilitiesLogger.WriteLine(LoggerType.Error, e.ToString());
            }
        }

        ///<summary>
        ///Clients initally connecting after their Discovery, lets send them a message
        ///if the server has not reached maximum inbound connections - send Accepting
        ///if the server has reached maximum connections - send NotAccepting
        ///if there was an error - send Failed
        //</summary>
        private async void ClientConnectedAsync(IPEndPoint address)
        {
            NetworkMessage sender = new NetworkMessage { MessageSenderIP = this.LocalEndpoint.Address.ToString(), MessageSenderPort = networkConfiguration.ReceivePort};

            try
            {
                if(this.Peers.GetPeerCount() < this.Peers.MaxInBound)
                {
                    sender.MessageStateType = MessageType.Accepting;           
                }
                else
                {
                   sender.MessageStateType = MessageType.NotAccepting;
                }
            }
            catch (Exception e)
            {
                sender.MessageStateType = MessageType.Failed;
                UtilitiesLogger.WriteLine(LoggerType.Error, e.ToString());                
            }

            await receiver.Send(sender, address);
        }

        private async void ServerMessageReceivedAsync(IPEndPoint sndrIp, NetworkMessage message)
        {
            UtilitiesLogger.WriteLine(LoggerType.Debug, "Receiver: message ({0}) received", message.MessageStateType.ToString());
            NetworkMessage sender = new NetworkMessage { MessageSenderIP = this.LocalEndpoint.Address.ToString(), MessageSenderPort = networkConfiguration.ReceivePort, KnownPeers = this.Peers.ConvertPeersToStringArray()};

            if (message.MessageStateType == MessageType.Connected)
            {   
                sender.MessageStateType = MessageType.Gab;                
                NetworkPeer networkPeers = new NetworkPeer(new IPEndPoint(IPAddress.Parse(message.MessageSenderIP), message.MessageSenderPort));
                var result = this.Peers.AddInboundPeer(networkPeers);
                this.Peers.CombinePeers(message.KnownPeers);               
                await receiver.Send(sender, sndrIp);
            }

            if (message.MessageStateType == MessageType.Gab)
            {
                sender.MessageStateType = MessageType.Gab;
                await receiver.Send(sender, sndrIp);
            }

            if (message.MessageStateType == MessageType.TurnRed)
            {
                sender.MessageStateType = MessageType.TurnRed;
                if (Console.BackgroundColor != ConsoleColor.Red)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.Clear();                    
                    await this.Peers.Broadcast(sender);
                }
            }

            if (message.MessageStateType == MessageType.AddBlock)
            {
                UtilitiesLogger.WriteLine(LoggerType.Info, "Receiver: receiver new block [{0}-{1}]", sender.Block.Poster, sender.Block.Post);
                this.BlockChain.AddBlock(sender.Block);
                await this.Peers.Broadcast(sender);
            }

            UtilitiesLogger.WriteLine(LoggerType.Debug, "Receiver: message ({0}) sent", sender.MessageStateType.ToString());           
            
        }

    }
}
