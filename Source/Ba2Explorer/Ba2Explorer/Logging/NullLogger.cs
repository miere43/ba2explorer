using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Explorer.Logging
{
    /// <summary>
    /// Represents a logger which logs messages into nowhere.
    /// </summary>
    internal class NullLogger : ILogger
    {
        public void Log(LogPriority priority, string message)
        {
        }

        public void Log(LogPriority priority, string format, params object[] args)
        {
        }

        public void Dispose()
        {
        }

        public void LogException(LogPriority priority, string source, Exception e)
        {
        }
    }
}
