using System;
using CommandLine;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Xerxes.P2P.Driver
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

        [Option('r', "receive", Required = true, HelpText = "The port the node is listening on.")]
        public int ReceivePort { get; set; }

        [Option('s', "seek", Required = false, Default=null, HelpText = "The port the node is seeking.")]
        public int? SeekPort { get; set; }
    }
}