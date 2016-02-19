using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TommiUtility.FileSystem
{
    public class LogWriter : TextWriter
    {
        public readonly TextWriter TextWriter;
        public LogWriter(TextWriter textWriter, string logPath)
        {
            Contract.Requires<ArgumentNullException>(textWriter != null);
            Contract.Requires<ArgumentNullException>(logPath != null);
            Contract.Requires<ArgumentException>(logPath.Length > 0);

            TextWriter = textWriter;
            LogPath = logPath;
        }

        public readonly string LogPath;
        public override Encoding Encoding { get { return Encoding.UTF8; } }
        private readonly StringBuilder cache = new StringBuilder();
        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(TextWriter != null);
            Contract.Invariant(LogPath != null);
            Contract.Invariant(LogPath.Length > 0);
            Contract.Invariant(cache != null);
        }

        public override void Write(char value)
        {
            TextWriter.Write(value);

            cache.Append(value);

            if (cache.Length > 1000)
            {
                Flush();
            }
        }
        public override void Flush()
        {
            TextWriter.Flush();

            File.AppendAllText(LogPath, cache.ToString(), Encoding);

            cache.Clear();
        }
    }
}
