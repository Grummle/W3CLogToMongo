using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using IILogReader;
using IILogReader.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using StructureMap;

namespace IISLogParser.CommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Process.GetCurrentProcess().PriorityClass=ProcessPriorityClass.BelowNormal;
            var config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(args[0]));
            Registry.Configure(config.logPath,args[0]);
            var logger = ObjectFactory.GetInstance<Logger>();
            config.sources.Select(x=>x.ProcessSource()).Where(x=>x.enabled.Value).ForEach(x =>
                                       {
                                           try
                                           {
                                               x.WriteJson();
                                               logger.Log("Start Processing Source: \"{0}\"".Frmat(x.id));
                                               ObjectFactory.GetInstance<LogProcessor>().Process(x);
                                               logger.Log("Finish Processing Source: \"{0}\"".Frmat(x.id));
                                           }
                                           catch (Exception e)
                                           {
                                               logger.Log("Well shit. This source:({0}) is being a pita.".Frmat(x.id),e);
                                           }
                                       });
        }
    }
}
