using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xerxes.Utils;
using GenericProtocol;
using GenericProtocol.Implementation;
using System.Linq;

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
        /// <summary>Task accepting new clients in a loop.</summary>
       
        public NetworkReceiver(INetworkConfiguration netConfig, UtilitiesConfiguration utilConf, ref NetworkPeers peers)
        {
            this.utilConf = utilConf;
            this.LocalEndpoint = NetworkDiscovery.GetEndPoint(netConfig.Turf, utilConf, netConfig.ReceivePort);
            this.receiver = new ProtoServer<NetworkMessage>(this.LocalEndpoint.Address, this.LocalEndpoint.Port);
            this.serverCancel = new CancellationTokenSource();
            this.networkConfiguration = netConfig;
            this.Peers = peers;
        }

        public async Task ReceivePeersAsync()    
        {
            try
            {
                await Task.Run(() =>
                {                    
                    receiver.Start();
                    //UtilitiesConsole.Update(UCommand.StatusInbound, "Server started on " + this.LocalEndpoint.ToString());
                    Console.WriteLine("From the receiver: Server started on " + this.LocalEndpoint.ToString());
                    receiver.ClientConnected += ClientConnectedAsync;
                    receiver.ReceivedMessage += ServerMessageReceivedAsync;
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private async void ClientConnectedAsync(IPEndPoint address)
        {
            try
            {
                NetworkMessage sender = new NetworkMessage { MessageSenderIP = IPAddress.Loopback.ToString(), MessageSenderPort = networkConfiguration.ReceivePort, MessageStateType = MessageType.Seek };
                await receiver.Send(sender, to:address);  
                Console.WriteLine("From the receiver: Peer Connected, Awaiting Message");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private async void ServerMessageReceivedAsync(IPEndPoint sndrIp, NetworkMessage message)
        {
            Console.WriteLine($"{message.MessageSenderPort}:{message.MessageSenderPort}");
            if(message.MessageStateType == MessageType.Seek){
                NetworkMessage sender = new NetworkMessage { MessageSenderIP = IPAddress.Loopback.ToString(), MessageSenderPort = networkConfiguration.ReceivePort, MessageStateType = MessageType.Created };
                //IPEndPoint sndTo = new IPEndPoint(IPAddress.Parse(message.MessageSenderIP), message.MessageSenderPort);
                await receiver.Send(sender, sndrIp);
                sndrIp = new IPEndPoint(IPAddress.Parse(message.MessageSenderIP), message.MessageSenderPort);
                NetworkPeer networkPeers = new NetworkPeer(sndrIp);
                var result = this.Peers.AddInboundPeer(networkPeers);
                //UtilitiesConsole.Update(UCommand.StatusInbound, "Handshake Sent");
                Console.WriteLine("From the receiver: Handshake Sent");
                //UtilitiesConsole.Update(UCommand.InBoundPeers, this.Peers.Count.ToString());
                Console.WriteLine("From the receiver: Peer Count {0}", this.Peers.Count.ToString());
            }
                
        }
  
    }
}
