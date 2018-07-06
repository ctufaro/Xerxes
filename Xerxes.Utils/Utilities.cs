using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Xerxes.Utils
{
    public class Utilities
    {
        public static string GetApplicationRoot(string filename = "")
        {
            var exePath =   Path.GetDirectoryName(System.Reflection
                            .Assembly.GetExecutingAssembly().CodeBase);
            Regex appPathMatcher=new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var appRoot = appPathMatcher.Match(exePath).Value;
            appRoot = string.IsNullOrEmpty(filename) ? appRoot : Path.Combine(appRoot, filename);
            return appRoot;
        }
    }
}