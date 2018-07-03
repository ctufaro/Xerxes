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
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Loopback, options.ReceivePort);
            NetworkReceiver networkReceiver = new NetworkReceiver(iPEndPoint);
            Task.Run(()=> networkReceiver.ReceivePeers(false));

            if(options.SeekPort!=null)
            {
                IPEndPoint singleSeekPoint = new IPEndPoint(IPAddress.Loopback, options.SeekPort.Value);
                NetworkSeeker networkSeeker = new NetworkSeeker();
                Task.Run(()=> networkSeeker.SeekPeers());
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
