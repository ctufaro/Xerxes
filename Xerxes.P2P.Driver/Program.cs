﻿using System;
using System.Net;
using CommandLine;
using System.Threading.Tasks;
using CommandLine.Text;

namespace Xerxes.P2P.Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            TryParseArgs(args);
        }

        private static void Start(Options options)
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Loopback, options.ReceivePort);
            NetworkReceiver networkReceiver = new NetworkReceiver(iPEndPoint);
            Task.Run(()=> networkReceiver.ReceivePeers(false));

            if(options.SeekPort!=null)
            {
                IPEndPoint singleSeekPoint = new IPEndPoint(IPAddress.Loopback, options.SeekPort.Value);
                NetworkSeeker networkSeeker = new NetworkSeeker(singleSeekPoint);
                Task.Run(()=> networkSeeker.SeekPeers());
            }

            if(options.Gossip!=null)
            {
                NetworkSeeker networkSeeker = new NetworkSeeker();
                Task.Run(()=> networkSeeker.StartInfectPeersAsync(options.Gossip, 5));
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
                Console.WriteLine(helpText);
            });
        }
    }
}
