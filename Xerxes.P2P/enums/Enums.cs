using System;

namespace Xerxes.P2P
{
    public enum Turf
    {
        Intranet = 1,
        TestNet = 2,
        MainNet = 3
    }

    public enum MessageType
    {
        /// <summary>Seeking peers.</summary>
        Seek = 0,
        /// <summary>Initial state of an outbound peer.</summary>
        Accepting,
        /// <summary>Network connection with the peer has been established.</summary>
        Connected,
        /// <summary>The node and the peer exchanged version information.</summary>
        HandShaked,
        /// <summary>Share Peers List.</summary>
        Share,
        /// <summary>Process of disconnecting the peer has been initiated.</summary>
        Disconnecting,
        /// <summary>Shutdown has been initiated, the node went offline.</summary>
        Offline,
        /// <summary>An error occurred during a network operation.</summary>
        Failed,
        /// <summary>Gab</summary>
        Gab,
        /// <summary> Not Accepting Connections</summary>
        NotAccepting,
        TurnRed,
        DownloadChain,
        AddBlock,
        RequestAge
    }  
}
