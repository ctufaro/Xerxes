using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Xerxes.Utils
{
    public sealed class UtilitiesLogger
    {
        private static Dictionary<string, Tuple<int,string>> shit = new Dictionary<string, Tuple<int, string>>();
        private static readonly Lazy<UtilitiesLogger> lazy = new Lazy<UtilitiesLogger>(() => new UtilitiesLogger());
    
        public static UtilitiesLogger Instance { get { return lazy.Value; } }
        private UCommand[] stats = new UCommand[]{UCommand.OutboundPeers,
                                                  UCommand.InBoundPeers,
                                                  UCommand.StatusOutbound,
                                                  UCommand.StatusInbound};
        private static int spCnt;

        private UtilitiesLogger()
        {
            PrintTitle();
            PrintStats();
        }

        public static void WriteLine(LoggerType @type, string format, params object[] parameters)
        {   
            if(type==LoggerType.Debug && System.Diagnostics.Debugger.IsAttached)
            {
                if(parameters.Length > 0)
                    Console.WriteLine(format, parameters);
                else
                    Console.WriteLine(format);            }
            else
            {
                if (type != LoggerType.Debug)
                {
                    if(parameters.Length > 0)
                        Console.WriteLine(format, parameters);
                    else
                        Console.WriteLine(format);
                }
            }
        }   

        private void PrintTitle()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"____  ___                                 ");
            sb.AppendLine(@"\   \/  /______________  ___ ____   ______");
            sb.AppendLine(@" \     // __ \_  __ \  \/  // __ \ /  ___/");
            sb.AppendLine(@" /     \  ___/|  | \/>    <\  ___/ \___ \ ");
            sb.AppendLine(@"/___/\  \___  >__|  /__/\_ \\___  >____  >");
            sb.AppendLine(@"      \_/   \/            \/    \/     \/ ");
            Console.Clear();
            spCnt = Regex.Matches(sb.ToString(), Environment.NewLine).Count+1;
            Console.WriteLine(sb.ToString());
        }

        private void PrintStats()
        {
            foreach(var stat in stats)
            {                
                shit.Add(stat.Value, new Tuple<int,string>(spCnt++,"*** " + stat.Value + ": {0}              "));
            }

            foreach(string stat in shit.Keys)
            {
                Console.WriteLine("*** " + stat + ": ");
            }
        }

        public static void Updates(UCommand key, string value)
        {
            var p = shit[key.Value];
            Console.SetCursorPosition(0, p.Item1);
            Console.WriteLine(p.Item2, value);
            Console.SetCursorPosition(0, spCnt);
        }

    }

    public enum LoggerType
    {
        Error,
        Fatal,
        Info,
        Debug
    }

    public class UCommand
    {
        private UCommand(string value) { Value = value; }
        public string Value { get; set; }        
        public static UCommand OutboundPeers { get { return new UCommand("Outbound Peers"); } }
        public static UCommand InBoundPeers { get { return new UCommand("Inbound Peers"); } }
        public static UCommand StatusOutbound { get { return new UCommand("Status (Outbound)"); } }
        public static UCommand StatusInbound { get { return new UCommand("Status (Inbound)"); } }
    }

}