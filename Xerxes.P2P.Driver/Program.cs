using System;

namespace Xerxes.P2P.Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 )
            {
                Console.WriteLine("Please specify (optional) server xxx.xxx.xxx.xxx and port xxxx in the command line.");
            }
            else
            {
                int port = 0;
                string address = null;

                if(args.Length == 1){
                    port = Int32.Parse(args[0]);
                }
                else if(args.Length == 2){
                    address = args[0];
                    port = Int32.Parse(args[1]);
                }

                NetworkPeerServer.StartServer(port,address); // Start the server  
                NetworkPeerServer.Listen(); // Start listening. 
            }
        }
    }
}
