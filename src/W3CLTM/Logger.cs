using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IILogReader
{
    public class Logger
    {
        public virtual string Path { get; set; }
        private int ExceptionCount { get; set; }
        private bool warned { get; set; }

        protected Logger() { }

        public Logger(string path)
        {
            Path = path;
        }

        public virtual void Log(string message)
        {
            using (var fs = File.AppendText(Path))
            {
                fs.WriteLine(message);
            }
        }

        public virtual void Log(string message, Exception exception)
        {
            var text = string.Format("{0}: {1}\r\n{2}", message, exception.GetType().Name, exception.ToString());
            if (ExceptionCount < 100)
            {
                Log(text);
                ExceptionCount++;
            }
            else if (!warned)
                Log("You've got major problems. Exception Logging discontinued.");
        }
    }
}
