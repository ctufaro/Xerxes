using System;
using System.Net;

namespace Xerxes.P2P
{
    /// <summary>    
    /// When a user first starts receiving, they need to broadcast their existence to street nodes
    /// When a user is seeking, they need to be notified when a new peer has joined the network and update
    /// their peer list.
    /// </summary>
    public class NetworkDiscovery
    {
        private INetworkConfiguration NetworkConfiguration;
        public NetworkDiscovery(INetworkConfiguration networkConfiguration)
        {
            this.NetworkConfiguration = networkConfiguration;
        }

        /// <summary>
        /// TODO: Refine this method, this method should: (1) initially route you to a street node
        /// once connected to a streetnode, the streetnode should share it's peers with you,
        /// you should then add those peers to your peers list. (2) You should then PERIODICALLY scan your peers lists and ask
        /// them for their peers, update your list.null (3) PERIODICALLY, street nodes should swap with other streets.
        /// After the swap, streets sould reshare with peers.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public IPEndPoint ConnectToStreet()
        {            
            IPEndPoint ipEndPoint = null;
            
            if(this.NetworkConfiguration.Turf == Turf.Intranet)
            {
                foreach(int port in DNSSeeds.Ports)
                {
                    ipEndPoint = new IPEndPoint(IPAddress.Loopback, port);
                    //send out NetworkStateType.Seek
                    break;
                }
            }
            else if(this.NetworkConfiguration.Turf == Turf.TestNet)
            {
                foreach(string name in DNSSeeds.Names)
                {
                    IPHostEntry host = Dns.GetHostEntry(name);
                    foreach (IPAddress ip in host.AddressList)
                    {
                        //send out NetworkStateType.Seek
                    }
                }
            }

            return ipEndPoint;
        }
    }
}