using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using Xerxes.Domain;
using ZeroFormatter;

namespace Xerxes.P2P
{
    [ZeroFormattable]
    public class NetworkMessage
    {        
        [Index(0)]
        public virtual string MessageSenderIP {get;set;}
        [Index(1)]
        public virtual int MessageSenderPort {get;set;}
        [Index(2)]
        public virtual MessageType MessageStateType {get;set;}
        [Index(3)]
        public virtual string[] KnownPeers { get; set; }
        [Index(4)]
        public virtual Block Block { get; set; }
        [Index(5)]
        public virtual BlockChain BlockChain { get; set; }
        [Index(6)]
        public virtual DateTime Age { get; set; }
    }        
        

 
 
}