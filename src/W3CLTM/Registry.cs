using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IILogReader.Configuration;
using StructureMap;

namespace IILogReader
{
    public static class Registry
    {
        public static void Configure(string logPath,string configPath)
        {
            ObjectFactory.Initialize(x=>
                                         {
                                             x.For<Logger>().Singleton().Use<Logger>().Ctor<string>("path").Is(logPath);
                                             x.For<StateRecorder>().Use<StateRecorder>().Ctor<string>("path").Is(configPath);
                                         });
        }
    }
}
