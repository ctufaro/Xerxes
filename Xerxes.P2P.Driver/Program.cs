using System;

namespace Xerxes.P2P.Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");            
            NetworkPeerServer.StartServer("192.168.1.12",5678); // Start the server  
            NetworkPeerServer.Listen(); // Start listening. 
        }
    }
}
