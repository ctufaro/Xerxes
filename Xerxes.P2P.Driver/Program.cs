using System;
using System.Net;
using System.Threading.Tasks;

namespace Xerxes.P2P.Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                int receivePort = Int32.Parse(args[0]);
                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Loopback, receivePort);
                NetworkReceiver networkReceiver = new NetworkReceiver(iPEndPoint);
                Task.Run(()=> networkReceiver.ReceivePeers());

                int seekPort = Int32.Parse(args[1]);
                IPEndPoint singleSeekPoint = new IPEndPoint(IPAddress.Loopback, seekPort);
                NetworkSeeker networkSeeker = new NetworkSeeker(singleSeekPoint);
                Task.Run(()=> networkSeeker.SeekPeers());

                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Two Arguments required <receiveport> <seekport>");
            }

            
        }
    }
}
