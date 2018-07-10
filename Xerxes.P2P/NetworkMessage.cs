using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace Xerxes.P2P
{
    public class NetworkMessage
    {
        
        public NetworkStateType MessageStateType{get;set;}

        public string MessageSenderIP{get;set;}

        public int MessageSenderPort{get;set;}
        public static byte[] NetworkMessageToByteArray(NetworkMessage obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
            
        }

        public static string NetworkMessageToJSON(NetworkMessage obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static NetworkMessage JSONToNetworkMessage(string json)
        {
            return JsonConvert.DeserializeObject<NetworkMessage>(json);
        }        

        public static NetworkMessage ByteArrayToNetworkMessage(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream) as NetworkMessage;
                return obj;
            }
        }        
    }

    public enum NetworkStateType
    {
        /// <summary>Seeking peers.</summary>
        Seek = 0,
        /// <summary>Initial state of an outbound peer.</summary>
        Created,
        /// <summary>Network connection with the peer has been established.</summary>
        Connected,
        /// <summary>The node and the peer exchanged version information.</summary>
        HandShaked,
        /// <summary>Process of disconnecting the peer has been initiated.</summary>
        Disconnecting,
        /// <summary>Shutdown has been initiated, the node went offline.</summary>
        Offline,
        /// <summary>An error occurred during a network operation.</summary>
        Failed
    }

    [Serializable]
    public enum NetworkMessageType
    {
        /// <summary>Simple Acknowledgement Message</summary>
        Ack = 0

    }
}