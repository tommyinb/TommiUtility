using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TommiUtility.Logging
{
    public class DailyLogger
    {
        public DailyLogger(string filePath)
        {
            Contract.Requires<ArgumentNullException>(filePath != null);
            Contract.Requires<ArgumentException>(filePath.Length > 0);

            directory = Path.GetDirectoryName(filePath);
            if (directory.Length <= 0) throw new ArgumentException();

            fileName = Path.GetFileName(filePath);
            if (fileName.Length <= 0) throw new ArgumentException();
        }
        private readonly string directory;
        private readonly string fileName;

        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(directory != null);
            Contract.Invariant(fileName != null);
        }

        public void Log(string message)
        {
            Log(new[] { message });
        }
        public void Log(string[] messages)
        {
            Contract.Requires<ArgumentNullException>(messages != null);

            if (directory.Length > 0)
            {
                Directory.CreateDirectory(directory);
            }

            var currTime = DateTime.Now;

            var todayFileName = currTime.ToString("yyMMdd") + "-" + fileName;
            var filePath = Path.Combine(directory, todayFileName);

            var timeTag = currTime.ToString("[HH:mm:ss] ");
            var lines = messages.Take(1).Select(t => timeTag + t).Concat(messages.Skip(1)).Select(t => t ?? string.Empty);

            File.AppendAllLines(filePath, lines);
        }

        public void Log(Exception exception)
        {
            Contract.Requires<ArgumentNullException>(exception != null);

            Log(new[] { exception.ToString() });
        }
        public void Log(string message, Exception exception)
        {
            Contract.Requires<ArgumentNullException>(exception != null);

            Log(new[] { message, exception.ToString() });
        }
    }
}
