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
                Start(new Options { Receive = true, ReceivePort = 1000, Seek = true, Turf = 1 });
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
            UtilitiesConsole UCon = UtilitiesConsole.Instance;
            networkConfiguration.Turf = (Turf)options.Turf.Value;
            networkConfiguration.ReceivePort = options.ReceivePort.Value;
            NetworkPeers peers = new NetworkPeers(utilConfiguration.GetOrDefault<int>("maxinbound", 117), utilConfiguration.GetOrDefault<int>("maxoutbound", 8));

            if (options.Receive.Value)
            {
                NetworkReceiver networkReceiver = new NetworkReceiver(networkConfiguration, utilConfiguration, ref peers); 
                Task.Run(()=> networkReceiver.ReceivePeersAsync());
            }            

            if(options.Seek.Value)
            {                
                NetworkSeeker networkSeeker = new NetworkSeeker(networkConfiguration, utilConfiguration, ref peers);
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
