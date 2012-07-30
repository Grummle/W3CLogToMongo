using System.Linq;
using IILogReader;
using IILogReader.Configuration;
using NUnit.Framework;
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