using System;
using System.Net;
using Xerxes.Utils;

namespace Xerxes.P2P
{
    /// <summary>    
    /// When a user first starts receiving, they need to broadcast their existence to street nodes
    /// When a user is seeking, they need to be notified when a new peer has joined the network and update
    /// their peer list.
    /// </summary>
    public class NetworkDiscovery
    {
        bool UseIntranetOnly = false;
        public NetworkDiscovery(bool useIntranetOnly)
        {
            this.UseIntranetOnly = useIntranetOnly;
        }

        public void BroadcastToStreet()
        {            
            if(UseIntranetOnly)
            {
                foreach(int port in DNSSeeds.Ports)
                {
                    //send out NetworkStateType.Seek
                }
            }
            else
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
        }
    }
}