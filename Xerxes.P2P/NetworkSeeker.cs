using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Xerxes.P2P
{
    public class NetworkSeeker
    {
        /// <summary>Cancellation that is triggered on shutdown to stop all pending operations.</summary>
        private readonly CancellationTokenSource serverCancel;
        /// <summary>List of all outbound peers.</summary>

        public NetworkSeeker()
        {
            this.serverCancel = new CancellationTokenSource();         
        }

        public void SeekPeers()
        {

        }

               

    }
}
