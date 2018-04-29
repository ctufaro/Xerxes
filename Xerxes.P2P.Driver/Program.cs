using System;

namespace Xerxes.P2P.Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Please specify server xxx.xxx.xxx.xxx and port xxxx in the command line.");
            }
            else
            {
                string server = args[0];
                int port = Int32.Parse(args[1]);
                NetworkPeerServer.StartServer("192.168.1.12", 5678); // Start the server  
                NetworkPeerServer.Listen(); // Start listening. 
            }
        }
    }
}
