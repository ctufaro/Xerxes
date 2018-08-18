using System;
using System.Net;
using System.Threading.Tasks;
using Xerxes.P2P;
using Xerxes.TCP.Implementation;

namespace Xerxes.TestNode
{
    public static class Program
    {
        private static ProtoServer<NetworkMessage> _server;
        private static ProtoClient<NetworkMessage> _client;

        private static readonly IPAddress ServerIp = IPAddress.Loopback;
        private static bool TestServer { get; } = false;
        private static bool TestClient { get; } = true;

        private static int myPort = 2000;
        private static int infectPort = 1233;

        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                await StartServerAsync();
            }).GetAwaiter().GetResult();

            Task.Run(async () =>
            {
                await StartClientAsync();
            }).GetAwaiter().GetResult();

            Console.WriteLine("Kennnnyyyy!!!");
            Console.ReadLine();
        }

        private static async Task StartClientAsync()
        {
            await ConnectClientAsync();

            NetworkMessage message = new NetworkMessage();
            message.MessageSenderIP = IPAddress.Loopback.ToString();
            message.MessageSenderPort = myPort;
            message.MessageStateType = MessageType.Gab;
            message.KnownPeers = new string[] { "" };

            await SendToServer(message);
        }

        private static async Task StartServerAsync()
        {
            await Task.Run(() =>
            {
                _server = new ProtoServer<NetworkMessage>(IPAddress.Any, myPort);
                Console.WriteLine("Starting Server...");
                _server.Start();
                Console.WriteLine("Server started!");
                _server.ClientConnected += _server_ClientConnected;
                _server.ReceivedMessage += _server_ReceivedMessage;
            }).ConfigureAwait(false);
        }

        private async static void _server_ReceivedMessage(IPEndPoint sndrIp, NetworkMessage message)
        {
            Console.WriteLine("Receiver: message ({0}) received", message.MessageStateType.ToString());
            NetworkMessage sender = new NetworkMessage { MessageSenderIP = IPAddress.Loopback.ToString(), MessageSenderPort = myPort, KnownPeers = new string[] { "" } };

            if (message.MessageStateType == MessageType.Connected)
            {
                sender.MessageStateType = MessageType.Gab;
                await _server.Send(sender, sndrIp);
            }

            if (message.MessageStateType == MessageType.Gab)
            {
                sender.MessageStateType = MessageType.Gab;
                await _server.Send(sender, sndrIp);
            }

            Console.WriteLine("Receiver: message ({0}) sent", sender.MessageStateType.ToString());
        }

        private async static void _server_ClientConnected(IPEndPoint endPoint)
        {
            NetworkMessage sender = new NetworkMessage();
            sender.MessageSenderIP = IPAddress.Loopback.ToString();
            sender.MessageSenderPort = myPort;
            sender.MessageStateType = MessageType.Accepting;
            sender.KnownPeers = new string[] { "" };
            await _server.Send(sender, endPoint);
        }

        private static async Task ConnectClientAsync()
        {
            _client = new ProtoClient<NetworkMessage>(ServerIp, infectPort) { AutoReconnect = true };
            _client.ReceivedMessage += ClientMessageReceived;
            _client.ConnectionLost += Client_ConnectionLost;

            NetworkMessage message = new NetworkMessage();
            message.MessageSenderIP = IPAddress.Loopback.ToString();
            message.MessageSenderPort = myPort;
            message.MessageStateType = MessageType.Connected;
            message.KnownPeers = new string[] { };

            Console.WriteLine("Connecting");
            await _client.Connect(true);
            Console.WriteLine("Connected!");
            await _client.Send(message);

            message.MessageStateType = MessageType.TurnRed;
            await _client.Send(message);
        }

        private static async Task SendToServer(NetworkMessage message)
        {
            await _client?.Send(message);
        }

        private static void Client_ConnectionLost(IPEndPoint endPoint)
        {
            Console.WriteLine($"Connection lost! {endPoint.Address}");
            Environment.Exit(1);
        }


        private static void ClientMessageReceived(IPEndPoint sender, NetworkMessage message)
        {
            Console.WriteLine($"{sender} {message.MessageStateType.ToString()}");
        }

    }
}
