using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IILogReader;
using IILogReader.Configuration;
using IISLogParser.CommandLine;
using MongoDB.Bson;
using MongoDB.Driver;
using NSubstitute;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap.TypeRules;

namespace IISLogReader.Tests.Integration
{
    [TestFixture]
    public class Crap
    {
        [Test]
        public void asdfasdfadsf()
        {
//            var stuff = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(@"..\..\integration\Configs\config.template.json"));
 //           stuff.sources.ForEach(x=>x.enabled.WriteLine());
            int? fark = 3;
            typeof (Configuration.source).GetProperties().Where(x => x.PropertyType.IsNullable()).ForEach(
                x => x.Name.WriteLine());

        }
    }


}
