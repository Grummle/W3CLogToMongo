using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IILogReader;
using IILogReader.Configuration;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace IISLogReader.Tests.Unit
{
    [TestFixture]
    public class ConfigurationLoaderTests
    {
        private W3CLogReaderFactory cl;
        private Configuration config;
        private Configuration.source source;

        [SetUp]
        public void Setup()
        {
            config = new Configuration(); ;
            source = new Configuration.source();
            source.aliases.Add(new Configuration.source.elementAlias{alias="theAlias",elementName = "theelemtnname"});
            source.conversions.Add(new Configuration.source.conversion{elementName = "LogTime",type = "DateTime"});
            source.composites.Add(new Configuration.source.composite{elementName="LogTime",elements = new []{"date","time"}});
            source.multiColumnElements.Add(new Configuration.source.mulitColumnElement{elementName = "Endtime-UTC",colSpan = 2});
            source.staticElements.Add(new Configuration.source.staticElement{elementName = "MyName",elementValue = "Dillon"});
            source.drop.Add("el1");
            source.drop.Add("el2");
            source.entryLimit = 100;

            config.sources.Add(source);

            cl = new W3CLogReaderFactory(Substitute.For<Logger>());
        }

        [Test]
        public void Should_Populate_Aliases()
        {
            var w3c = cl.CreateLogReader(source);

            w3c.Aliases.Count().ShouldBe(1);
            w3c.Aliases.First().Key.ShouldBe("theAlias");
            w3c.Aliases.First().Value.ShouldBe("theelemtnname");
        }

        [Test]
        public void Should_Populate_Conversions()
        {
            var w3c = cl.CreateLogReader(source);

            w3c.Conversions.Count().ShouldBe(1);
            w3c.Conversions.First().ElementName.ShouldBe("LogTime");
            w3c.Conversions.First().DataType.ShouldBe(typeof(DateTime));
        }

        [Test]
        public void Should_Populate_Composites()
        {
            var w3c = cl.CreateLogReader(source);

            w3c.Composites.Count().ShouldBe(1);
            w3c.Composites.First().Name.ShouldBe("LogTime");
            w3c.Composites.First().Components.SequenceEqual(source.composites.First().elements);
        }

        [Test]
        public void Should_Populate_MultiColumn()
        {
            var w3c = cl.CreateLogReader(source);

            w3c.MultiColumnElements.Count().ShouldBe(1);
            w3c.MultiColumnElements.First().Key.ShouldBe("Endtime-UTC");
            w3c.MultiColumnElements.First().Value.ShouldBe(2);
        }

        [Test]
        public void Should_Populate_Static_Elements()
        {
            var w3c = cl.CreateLogReader(source);

            w3c.StaticElements.Count().ShouldBe(1);
            w3c.StaticElements.First().Item1.ShouldBe("MyName");
            w3c.StaticElements.First().Item2.ShouldBe("Dillon");
        }

        [Test]
        public void Should_Populate_Drop_Elements()
        {
            var w3c = cl.CreateLogReader(source);

            w3c.Removals.Count().ShouldBe(2);
            w3c.Removals.SequenceEqual(new string[] {"e1", "e2"});
        }

        [Test]
        public void Should_Set_Entry_Limit()
        {
            var w3c = cl.CreateLogReader(source);

            w3c.EntryLimit.ShouldBe(100);
        }
    }
}
