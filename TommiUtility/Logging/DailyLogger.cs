using System;
using System.Collections.Generic;
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
            this.directory = Path.GetDirectoryName(filePath);
            this.fileName = Path.GetFileName(filePath);
        }
        private string directory;
        private string fileName;

        public void Log(string message)
        {
            Log(new[] { message });
        }
        public void Log(string[] messages)
        {
            if (directory.Length > 0)
            {
                Directory.CreateDirectory(directory);
            }

            var currTime = DateTime.Now;

            var todayFileName = currTime.ToString("yyMMdd") + "-" + fileName;
            var filePath = Path.Combine(directory, todayFileName);

            var timeTag = currTime.ToString("[HH:mm:ss] ");
            var lines = messages.Take(1).Select(t => timeTag + t).Concat(messages.Skip(1));

            File.AppendAllLines(filePath, lines);
        }

        public void Log(Exception exception)
        {
            Log(new[] { exception.ToString() });
        }
        public void Log(string message, Exception exception)
        {
            Log(new[] { message, exception.ToString() });
        }
    }
}
