using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TommiUtility.FileSystem
{
    public class LogWriter : TextWriter
    {
        public TextWriter TextWriter { get; private set; }
        public LogWriter(TextWriter textWriter, string logPath)
        {
            TextWriter = textWriter;

            LogPath = logPath;
        }

        public string LogPath { get; private set; }
        public override Encoding Encoding { get { return Encoding.UTF8; } }

        private StringBuilder cache = new StringBuilder();
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
