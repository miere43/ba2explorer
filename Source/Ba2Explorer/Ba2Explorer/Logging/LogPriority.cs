using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Explorer.Logging
{
    internal enum LogPriority
    {
        Info    = 0,
        Warning = 100,
        Error   = 200
    }

    internal static class LogPriorityExtensions
    {
        internal static bool IsLowerPriorityThan(this LogPriority @this, LogPriority other)
        {
            return (int)@this > (int)other;
        }
    }
}
