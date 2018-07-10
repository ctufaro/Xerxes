using System;

namespace Xerxes.P2P
{
    public interface INetworkConfiguration
    {
        Turf Turf{get;set;}
        int ReceivePort{get;set;} 
    } 
}