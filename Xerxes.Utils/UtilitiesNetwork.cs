using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Xerxes.Utils
{
    public class UtilitiesNetwork
    {
        public static string GetApplicationRoot(string filename = "")
        {
            var exePath =   Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            Regex appPathMatcher=new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var appRoot = appPathMatcher.Match(exePath).Value;
            appRoot = string.IsNullOrEmpty(filename) ? appRoot : Path.Combine(appRoot, filename);
            return appRoot;
        }

        public static IPAddress GetMyIPAddress()
        {
            //TODO: make this better, lol
            try
            {   
                string externalIP = new WebClient().DownloadString(@"http://icanhazip.com").Trim();
                return IPAddress.Parse(externalIP);
            }
            catch
            {

            }
            
            return IPAddress.Parse("0.0.0.0");
        }

        public static IEnumerable<IPAddress> GetRandomSeedNodes(string[] dnsnames, int throttle)
        {                
                List<IPAddress> retAddresses = new List<IPAddress>();
                foreach(string name in dnsnames)
                {
                    IPHostEntry host = Dns.GetHostEntry(name);
                    foreach (IPAddress ip in host.AddressList)
                    {
                        retAddresses.Add(ip);
                    }
                }

                if(throttle > retAddresses.Count) throttle = retAddresses.Count;

                List<int> randomNumbers = UtilitiesGeneral.GenerateUniqueRandomNumbers(throttle, retAddresses.Count);

                foreach(int rand in randomNumbers)
                {
                    yield return retAddresses[rand];
                }                
        }

        public static IEnumerable<IPEndPoint> GetRandomSeedPorts(string[] ports, int throttle)
        {            
            if(throttle > ports.Length) throttle = ports.Length;
            List<IPEndPoint> retPorts = new List<IPEndPoint>();
            foreach(string port in ports)
            {
                retPorts.Add(new IPEndPoint(IPAddress.Loopback, Int32.Parse(port)));            
            }

            List<int> randomNumbers = UtilitiesGeneral.GenerateUniqueRandomNumbers(throttle, retPorts.Count);
            
            foreach(int rand in randomNumbers)
            {
                yield return retPorts[rand];
            } 
        }

    }
}