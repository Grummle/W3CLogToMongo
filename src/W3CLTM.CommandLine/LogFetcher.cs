using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IILogReader;

namespace IISLogParser.CommandLine
{
    public class LogFetcher
    {
        public virtual IEnumerable<FileInfo> GetLogFiles(string path, string lastFileName, bool fallbackReturnAll = false)
        {
            var files = new DirectoryInfo(path).GetFiles().OrderByDescending(x => x.Name);

            if (lastFileName.IsNotNullOrEmpty())
            {
                if (!fallbackReturnAll && !files.Any(x => x.Name == lastFileName))
                    return files.First().toEnumerable();

                return files.TakeWhile(x => string.Compare(x.Name, lastFileName) >= 0).OrderBy(x=>x.Name);
            }

            if (!fallbackReturnAll)
                return files.First().toEnumerable();

            return files.OrderBy(x=>x.Name);
        }

        public virtual IEnumerable<string> GetLines(FileInfo file)
        {
                return new StreamReader(new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.Default).ReadLines();
        }
    }
}