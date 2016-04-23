using System;

namespace Ba2Explorer.Logging
{
    /// <summary>
    /// Represents simple logger interface.
    /// </summary>
    internal interface ILogger : IDisposable
    {
        void Log(LogPriority priority, string message);

        void Log(LogPriority priority, string format, params object[] args);

        void LogException(LogPriority priority, string source, Exception e);
    }
}
