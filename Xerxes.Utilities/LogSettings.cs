using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
namespace Xerxes.Utilities
{
    /// <summary>
    /// Configuration related to logging.
    /// </summary>
    public class LogSettings
    {
        /// <summary>
        /// Initializes an instance of the object with default values.
        /// </summary>
        public LogSettings()
        {
            this.DebugArgs = new List<string>();
            this.LogLevel = LogLevel.Information;
        }

        /// <summary>List of categories to enable debugging information for.</summary>
        /// <remarks>A special value of "1" of the first category enables trace level debugging information for everything.</remarks>
        public List<string> DebugArgs { get; private set; }

        /// <summary>Level of logging details.</summary>
        public LogLevel LogLevel { get; private set; }

       
    }
}
