using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using ZeroFormatter;

namespace Xerxes.P2P
{
    [ZeroFormattable]
    public struct NetworkMessage
    {        
        [Index(0)]
        public string MessageSenderIP{get;set;}
        [Index(1)]
        public int MessageSenderPort{get;set;}
        [Index(2)]
        public int MessageStateType{get;set;}
    }        
        

 
 
}