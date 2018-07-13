using System;
using CommandLine;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Xerxes.Driver
{
    public class Options
    {
        // [Option('r', "read", Required = true, HelpText = "Input files to be processed.")]
        // public IEnumerable<string> InputFiles { get; set; }

        // // Omitting long name, defaults to name of property, ie "--verbose"
        // [Option(Default = false, HelpText = "Prints all messages to standard output.")]
        // public bool Verbose { get; set; }

        // [Option("stdin", Default = false, HelpText = "Read from stdin")]
        // public bool stdin { get; set; }

        // [Value(0, MetaName = "offset", HelpText = "File offset.")]
        // public long? Offset { get; set; }

        [Option('r', "receive", Required = true, HelpText = "Flag specifying if accept incoming connections (ex: true/false).")]
        public bool? Receive { get; set; }

        [Option('v', "receive port", Required = true, HelpText = "The port the node is receiving.")]
        public int? ReceivePort { get; set; }        

        [Option('s', "seek", Required = true, Default=null, HelpText = "Flag specifying if you seek outbound connections (ex: true/false).")]
        public bool? Seek { get; set; }        
        
        [Option('p', "seek port", Required = false, Default=null, HelpText = "The port the node is seeking.")]
        public int? SeekPort { get; set; }

        [Option('t', "turf", Required = true, Default=null, HelpText = "The turf: 1 = intranet, 2 = testnet, 3 = mainnet.")]
        public int? Turf { get; set; }
    }
}