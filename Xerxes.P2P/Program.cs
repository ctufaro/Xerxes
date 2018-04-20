using System;
using System.Globalization;
using System.Net;
using Microsoft.Extensions.Logging;
using NBitcoin;

namespace Xerxes.P2P
{
    class Program
    {
        static void Main(string[] args)
        {
            ILoggerFactory loggerFactory = new LoggerFactory().AddConsole();
            ILogger logger = loggerFactory.CreateLogger<Program>();

            Network network = Network.RegTest;
            IPEndPoint localEndPoint = CreateIPEndPoint("0.0.0.0:18444");
            IPEndPoint externalEndPoint = CreateIPEndPoint("127.0.0.1:18444");
            NBitcoin.Protocol.ProtocolVersion version = NBitcoin.Protocol.ProtocolVersion.PROTOCOL_VERSION;
            NetworkPeerServer np = new NetworkPeerServer(network, localEndPoint, externalEndPoint, version, loggerFactory);
        }

        public static IPEndPoint CreateIPEndPoint(string endPoint)
        {
            string[] ep = endPoint.Split(':');
            if (ep.Length != 2) throw new FormatException("Invalid endpoint format");
            IPAddress ip;
            if (!IPAddress.TryParse(ep[0], out ip))
            {
                throw new FormatException("Invalid ip-adress");
            }
            int port;
            if (!int.TryParse(ep[1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port))
            {
                throw new FormatException("Invalid port");
            }
            return new IPEndPoint(ip, port);
        }
    }
}
