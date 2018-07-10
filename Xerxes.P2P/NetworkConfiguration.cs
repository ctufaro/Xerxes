using System;

namespace Xerxes.P2P
{
    public class NetworkConfiguration : INetworkConfiguration
    {
        public Turf Turf { get ; set; }
        public int ReceivePort {get; set;}
        public NetworkConfiguration()
        {
            
        }
    }
}