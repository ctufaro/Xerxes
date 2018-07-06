using System;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.Common;
using CommandLine;
using CommandLine.Text;
using Xerxes.Utils;
using Xerxes.P2P;

namespace Xerxes.Driver
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
            string config = File.ReadAllText(UtilitiesGeneral.GetApplicationRoot("Xerxes.conf"));
            UtilitiesConfiguration utilConfiguration = new UtilitiesConfiguration(config);
            INetworkConfiguration networkConfiguration = new NetworkConfiguration();
            networkConfiguration.Turf = (Turf)options.Turf.Value;
            
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Loopback, options.ReceivePort);
            NetworkReceiver networkReceiver = new NetworkReceiver(iPEndPoint);
            Task.Run(()=> networkReceiver.ReceivePeers());

            if(options.Seek.Value)
            {
                NetworkSeeker networkSeeker = new NetworkSeeker(networkConfiguration);
                Task.Run(()=> networkSeeker.SeekPeersAsync());
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
