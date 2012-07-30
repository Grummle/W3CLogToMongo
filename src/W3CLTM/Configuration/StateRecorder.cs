using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace IILogReader.Configuration
{
    public class StateRecorder
    {
        public IList<LogReaderState> States { get; set; }
        protected string filePath { get; set; }

        public StateRecorder()
        {
            States = new List<LogReaderState>();
        }

        public StateRecorder(string path)
        {
            filePath = Path.Combine(Path.GetDirectoryName(path), "state.json");
            States = new List<LogReaderState>();
            if (File.Exists(filePath))
            {
                States = JsonConvert.DeserializeObject<List<LogReaderState>>(File.ReadAllText(filePath));
            }
        }

        public virtual LogReaderState this[string id]
        {
            get
            {
                return States.FirstOrDefault(x=>x.id==id);
            }
            set
            {
                States.Where(x => x.id == value.id).ToList().ForEach(x => States.Remove(x));
                value.id = id;
                States.Where(x=>x.id==id).ToList().ForEach(x=>States.Remove(x));
                States.Add(value);
                File.WriteAllText(filePath,JsonConvert.SerializeObject(States,Formatting.Indented));
            }
        }
    }
    public class LogReaderState
    {
        public string id { get; set; }
        public string lastFile { get; set; }
        public int lastLine { get; set; }
        public IList<string> fields { get; set; }
    }
}
