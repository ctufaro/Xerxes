using System;
using System.Net;
using CommandLine;
using System.Threading.Tasks;
using CommandLine.Text;
using System.Diagnostics;

namespace Xerxes.P2P.Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Debugger.IsAttached)
            {
                Start(new Options { ReceivePort = 1111 });
            }
            else
            {
                TryParseArgs(args);
            }
        }

        private static void Start(Options options)
        {
            INetworkConfiguration networkConfiguration = new NetworkConfiguration();
            networkConfiguration.Turf = (Turf)options.Turf.Value;
            
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Loopback, options.ReceivePort);
            NetworkReceiver networkReceiver = new NetworkReceiver(iPEndPoint);
            Task.Run(()=> networkReceiver.ReceivePeers(false));

            if(options.Seek.Value)
            {
                IPEndPoint singleSeekPoint = (options.SeekPort !=null) ? new IPEndPoint(IPAddress.Loopback, options.SeekPort.Value) : null;
                NetworkSeeker networkSeeker = new NetworkSeeker(networkConfiguration);
                Task.Run(()=> networkSeeker.SeekPeersAsync(singleSeekPoint));
            }

            Console.ReadLine();
        }

        private static void TryParseArgs(string[] args)
        {
            var parserResult = CommandLine.Parser.Default.ParseArguments<Options>(args);
            parserResult.WithParsed<Options>(opts => Start(opts));
            parserResult.WithNotParsed<Options>(errs =>
            {
                var helpText = HelpText.AutoBuild(parserResult, h =>
                {
                    return HelpText.DefaultParsingErrorsHandler(parserResult, h);
                }, e =>
                {
                    return e;
                });                
            });
        }
    }
}
