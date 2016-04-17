using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Explorer.Logging
{
    /// <summary>
    /// Represents a logger that logs messages to stream.
    /// </summary>
    internal class FileLogger : ILogger
    {
        private StreamWriter writer;

        private FileStream output;

        public int LogMaxSize { get; set; } = 1024 * 1024;

        public FileLogger(FileStream target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            this.output = target;
            if (target.Length > LogMaxSize) // todo: test this
            {
                target.SetLength(0);
                target.Seek(0, SeekOrigin.Begin);
            }

            this.writer = new StreamWriter(target, Encoding.UTF8);
        }

        ~FileLogger()
        {
            Dispose(false);
        }

        public void Close()
        {
            ThrowIfDisposed();

            Dispose(false);
        }

        public void Log(string message, LogPriority priority)
        {
            ThrowIfDisposed();

            writer.WriteLine("[{0} {1}] {2}",
                priority.ToString(),
                DateTime.UtcNow.ToString("G", DateTimeFormatInfo.InvariantInfo),
                message);
        }

        public void Log(string format, LogPriority priority, params object[] args)
        {
            ThrowIfDisposed();

            writer.WriteLine("[{0} {1}] {2}",
                priority.ToString(),
                DateTime.UtcNow.ToString("G", DateTimeFormatInfo.InvariantInfo),
                string.Format(format, args));
        }

        private void ThrowIfDisposed()
        {
            // disposed already
            if (output == null)
                throw new ObjectDisposedException(nameof(FileLogger) + " is disposed.");
        }

        private void Dispose(bool disposeManaged)
        {
            // disposed already
            if (output == null)
                return;

            writer.Flush();
            output.Flush();
            writer.Dispose();
            output.Dispose();
            writer = null;
            output = null;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
