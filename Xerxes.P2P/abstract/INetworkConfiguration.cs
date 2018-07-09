using System;

namespace Xerxes.P2P
{
    public interface INetworkConfiguration
    {
        Turf Turf{get;set;}
        bool Street{get;set;}        
    } 
}