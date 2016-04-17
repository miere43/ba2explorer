using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Explorer.Logging
{
    /// <summary>
    /// Represents simple logger interface.
    /// </summary>
    internal interface ILogger : IDisposable
    {
        void Log(string message, LogPriority priority);

        void Log(string format, LogPriority priority, params object[] args);
    }
}
