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
        private static Random r = new Random();
        
        /// <summary>Cancellation that is triggered on shutdown to stop all pending operations.</summary>
        private readonly CancellationTokenSource serverCancel;

        private INetworkConfiguration networkConfiguration;

        private UtilitiesConfiguration utilConf;                

        /// <summary>TCP server listener accepting inbound connections.</summary>
        private ProtoServer<string> receiver;     

        /// <summary>IP address and port, on which the server listens to incoming connections.</summary>
        public IPEndPoint LocalEndpoint { get; private set; }
        
        /// <summary>List of all inbound peers.</summary>
        private NetworkPeers Peers;
        /// <summary>Task accepting new clients in a loop.</summary>
       
        public NetworkReceiver(INetworkConfiguration netConfig, UtilitiesConfiguration utilConf)
        {
            this.utilConf = utilConf;
            this.LocalEndpoint = GetEndPoint(netConfig.Turf, utilConf, netConfig.ReceivePort);
            this.receiver = new ProtoServer<string>(this.LocalEndpoint.Address, this.LocalEndpoint.Port);
            this.serverCancel = new CancellationTokenSource();
            this.networkConfiguration = netConfig;                        
            this.Peers = new NetworkPeers(utilConf.GetOrDefault<int>("maxinbound",117), utilConf.GetOrDefault<int>("maxoutbound",8));            
        }

        public async Task ReceivePeersAsync()    
        {
            try
            {
                await Task.Run(() =>
                {                    
                    receiver.Start();
                    UtilitiesConsole.Update(UCommand.StatusInbound, "Server started on " + this.LocalEndpoint.ToString());
                    receiver.ClientConnected += ClientConnectedAsync;
                    receiver.ReceivedMessage += ServerMessageReceivedAsync;
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public async void ClientConnectedAsync(IPEndPoint address)
        {
            try
            {
                
                UtilitiesConsole.Update(UCommand.StatusInbound, "ClientConnectedAsync Receiver Null? " + (receiver==null)); 
                await receiver.Send(message:"0", to:address);
                UtilitiesConsole.Update(UCommand.StatusInbound, "Peer Connected, Awaiting Message");                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private async void ServerMessageReceivedAsync(IPEndPoint sender, string message)
        {
            Console.WriteLine($"{sender}: {message}");
            if(message.Equals("0")){
                await receiver.Send("1", sender);
                NetworkPeer networkPeers = new NetworkPeer(sender);
                var result = this.Peers.AddInboundPeer(networkPeers);
                UtilitiesConsole.Update(UCommand.StatusInbound, "Handshake Sent");
                UtilitiesConsole.Update(UCommand.InBoundPeers, this.Peers.Count.ToString());              
            }
                
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
