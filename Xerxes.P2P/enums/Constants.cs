using System;

namespace Xerxes.P2P
{
    public class DNSSeeds
    {
        public static readonly string[] Names = new string[] {"bleecker.sytes.net", "delancey.sytes.net", "mercer.sytes.net"};
        public static readonly int[] Ports = new int[] {1234};
    }

    public enum Turf
    {
        Intranet = 1,
        TestNet = 2,
        MainNet = 3
    }  
}
