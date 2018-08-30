using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Red;
using Xerxes.Domain;
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

        #region TCP
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

            Task.Run(async () =>
            {
                await StartWebSocketAsync();
            }).GetAwaiter().GetResult();

            Console.ReadLine();
        }

        private static async Task StartClientAsync()
        {
            await ConnectClientAsync();

            NetworkMessage message = new NetworkMessage();
            message.MessageSenderIP = IPAddress.Loopback.ToString();
            message.MessageSenderPort = myPort;
            message.MessageStateType = MessageType.HandShake;
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
                sender.MessageStateType = MessageType.HandShake;
                await _server.Send(sender, sndrIp);
            }

            if (message.MessageStateType == MessageType.HandShake)
            {
                sender.MessageStateType = MessageType.HandShake;
                await _server.Send(sender, sndrIp);
            }

            if (message.MessageStateType == MessageType.AddBlock)
            {
                foreach (WebSocketDialog wd in wdcollection)
                {
                    if (wd.UnderlyingWebSocket.State == System.Net.WebSockets.WebSocketState.Open)
                    {
                        Block b = message.Block;
                        DownChain.MasterChain.Add(b);
                        string json = JsonConvert.SerializeObject(new { Index = b.Index, Prevhash = b.PrevHash, TimeStamp = b.TimeStamp, Poster = b.Poster, Post = b.Post });
                        await wd.SendText(json);
                    }
                }
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
            if (message.MessageStateType == MessageType.DownloadChain)
            {
                DownChain = message.BlockChain;
                foreach (Block b in message.BlockChain)
                {
                    foreach (WebSocketDialog wd in wdcollection)
                    {
                        if (wd.UnderlyingWebSocket.State == System.Net.WebSockets.WebSocketState.Open)
                        {
                            string json = JsonConvert.SerializeObject(new { Index = b.Index, Prevhash = b.PrevHash, TimeStamp = b.TimeStamp, Poster = b.Poster, Post = b.Post });
                            wd.SendText(json);
                        }
                    }
                }                
            }
            

            Console.WriteLine("sending back to server");
        }
        #endregion

        #region WebSocket https://github.com/rosenbjerg/Red https://www.yougetsignal.com
        private static List<WebSocketDialog> wdcollection = new List<WebSocketDialog>();
        private static BlockChain DownChain = new BlockChain();
        
        static async Task StartWebSocketAsync()
        {
            Console.WriteLine("Starting WebSocket");
            var server = new RedHttpServer(5001);
            DownChain.MasterChain = new List<Block>();
            server.RespondWithExceptionDetails = true;
            server.WebSocket("/echo", async (req, res, wsd) =>
            {
                wdcollection.Add(wsd);
                if (DownChain.Count() == 0)
                {
                    await DownloadChain();
                }
                else
                {
                    foreach (Block b in DownChain)
                    {
                        foreach (WebSocketDialog wd in wdcollection)
                        {
                            if (wd.UnderlyingWebSocket.State == System.Net.WebSockets.WebSocketState.Open)
                            {
                                string json = JsonConvert.SerializeObject(new { Index = b.Index, Prevhash = b.PrevHash, TimeStamp = b.TimeStamp, Poster = b.Poster, Post = b.Post });
                                await wd.SendText(json);
                            }
                        }
                    }
                }
                wsd.OnTextReceived += Wsd_OnTextReceived;
            });

            //await server.RunAsync();
            await server.RunAsync(new string[] { "192.168.1.5" });
            //await server.RunAsync(new string[] { "192.168.78.135" });
        }

        private static async Task DownloadChain()
        {
            NetworkMessage message = new NetworkMessage();
            message.MessageSenderIP = IPAddress.Loopback.ToString();
            message.MessageSenderPort = myPort;
            message.MessageStateType = MessageType.Connected;
            message.KnownPeers = new string[] { };
            message.MessageStateType = MessageType.DownloadChain;
            await _client.Send(message);
        }

        private static void Wsd_OnTextReceived(object sender, WebSocketDialog.TextMessageEventArgs e)
        {
            //Console.WriteLine(e.Text);
            Task.Run(async () =>
            {
                NetworkMessage message = new NetworkMessage();
                message.MessageSenderIP = IPAddress.Loopback.ToString();
                message.MessageSenderPort = myPort;
                message.MessageStateType = MessageType.Connected;
                message.KnownPeers = new string[] { };
                message.MessageStateType = MessageType.AddBlock;                
                message.Block = new Block(Guid.NewGuid().ToString(), "Webserver", e.Text);            
                await _client.Send(message);
            }).ConfigureAwait(false);
            Console.WriteLine("Message: {0} from webserver sent to nodes", e.Text);
        }
        #endregion
    }
}
