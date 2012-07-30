using System;
using System.IO;

namespace IILogReader
{
    public class Logger
    {
        protected Logger()
        {
        }

        public Logger(string path)
        {
            Path = path;
        }

        public virtual string Path { get; set; }
        private int ExceptionCount { get; set; }
        private bool warned { get; set; }

        public virtual void Log(string message)
        {
            using (StreamWriter fs = File.AppendText(Path))
            {
                fs.WriteLine(message);
            }
        }

        public virtual void Log(string message, Exception exception)
        {
            string text = string.Format("{0}: {1}\r\n{2}", message, exception.GetType().Name, exception);
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