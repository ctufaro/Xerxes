using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Xerxes.Utils
{
    public class UtilitiesGeneral
    {
        public static string GetApplicationRoot(string filename = "")
        {
            var exePath =   Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            Regex appPathMatcher=new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var appRoot = appPathMatcher.Match(exePath).Value;
            appRoot = string.IsNullOrEmpty(filename) ? appRoot : Path.Combine(appRoot, filename);
            return appRoot;
        }

        public static List<int> GenerateListOfRandomNumbers(int count, int maxValue){
            ThreadSafeRandom random = new ThreadSafeRandom(maxValue);
            List<int> randomNumbers = new List<int>();
            for(int i =0; i<count; i++){
                randomNumbers.Add(random.Next());
            }
            return randomNumbers;
        }
    }

    public class ThreadSafeRandom
    {
        private int maxValue;
        private static readonly Random _global = new Random();
        [ThreadStatic] private static Random _local;

        public ThreadSafeRandom(int maxValue)
        {
            this.maxValue = maxValue;
            if (_local == null)
            {
                int seed;
                lock (_global)
                {
                    seed = _global.Next();
                }
                _local = new Random(seed);
            }
        }
        public int Next()
        {
            return _local.Next(1, maxValue);
        }
    }
}