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
        void Log(LogPriority priority, string message);

        void Log(LogPriority priority, string format, params object[] args);
    }
}
