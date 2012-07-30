using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IILogReader;
using IILogReader.Configuration;
using Newtonsoft.Json;
using StructureMap;

namespace IISLogParser.CommandLine
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
            var config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(args[0]));
            Registry.Configure(config.logPath, args[0]);
            var logger = ObjectFactory.GetInstance<Logger>();
            config.sources.Select(x => x.ProcessSource()).Where(x => x.enabled.Value).ForEach(x =>
                                                                                                  {
                                                                                                      try
                                                                                                      {
                                                                                                          x.WriteJson();
                                                                                                          logger.Log(
                                                                                                              "Start Processing Source: \"{0}\""
                                                                                                                  .Frmat
                                                                                                                  (x.id));
                                                                                                          ObjectFactory.
                                                                                                              GetInstance
                                                                                                              <
                                                                                                                  LogProcessor
                                                                                                                  >().
                                                                                                              Process(x);
                                                                                                          logger.Log(
                                                                                                              "Finish Processing Source: \"{0}\""
                                                                                                                  .Frmat
                                                                                                                  (x.id));
                                                                                                      }
                                                                                                      catch (Exception e
                                                                                                          )
                                                                                                      {
                                                                                                          logger.Log(
                                                                                                              "Well shit. This source:({0}) is being a pita."
                                                                                                                  .Frmat
                                                                                                                  (x.id),
                                                                                                              e);
                                                                                                      }
                                                                                                  });
        }
    }
}