using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IILogReader;
using IILogReader.Configuration;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Shouldly;

namespace IISLogReader.Tests.Integration
{
    [TestFixture]
    public class W3CLogReaderFactoryTests
    {
        public Configuration.source source;
        [SetUp]
        public void Setup()
        {
            source = new Configuration.source(); 
        }

        [Test]
        public void Confirm_source_Defaults()
        {
            var vanillaSource = new Configuration.source();

            vanillaSource.aliases.Any().ShouldBe(false);
            vanillaSource.conversions.Any().ShouldBe(false);
            vanillaSource.composites.Any().ShouldBe(false);
            vanillaSource.drop.Any().ShouldBe(false);
            vanillaSource.multiColumnElements.Any().ShouldBe(false);
            vanillaSource.staticElements.Any().ShouldBe(false);
            vanillaSource.templates.Any().ShouldBe(false);

            vanillaSource.batchSize.GetValueOrDefault().ShouldBe(0);
            vanillaSource.enabled.GetValueOrDefault().ShouldBe(false);
            vanillaSource.entryLimit.GetValueOrDefault().ShouldBe(0);
            vanillaSource.id.ShouldBe(null);
            vanillaSource.logAll.GetValueOrDefault().ShouldBe(false);
            vanillaSource.logDirectory.ShouldBe(null);

            vanillaSource.destination.ShouldBe(null);
        }

        [Test]
        public void Should_Over_Lay_Default()
        {
            var baseSource = new Configuration.source();
            var overlay = new Configuration.source();

            baseSource.enabled.GetValueOrDefault().ShouldBe(false);
            overlay.enabled = true;
            baseSource.Overlay(overlay);
            baseSource.enabled.GetValueOrDefault().ShouldBe(true);

            baseSource.batchSize.GetValueOrDefault().ShouldBe(0);
            overlay.batchSize= 69;
            baseSource.Overlay(overlay);
            baseSource.batchSize.GetValueOrDefault().ShouldBe(69);

            baseSource.id.IsNullOrEmpty().ShouldBe(true);
            overlay.id= "anid";
            baseSource.Overlay(overlay);
            baseSource.id.ShouldBe("anid");

            baseSource.logDirectory.IsNullOrEmpty().ShouldBe(true);
            overlay.logDirectory= "logdir";
            baseSource.Overlay(overlay);
            baseSource.logDirectory.ShouldBe("logdir");

            baseSource.entryLimit.GetValueOrDefault().ShouldBe(0);
            overlay.entryLimit= 1000;
            baseSource.Overlay(overlay);
            baseSource.entryLimit.ShouldBe(1000);

            baseSource.destination.IsNull().ShouldBe(true);
            overlay.destination = new Configuration.source.logDestination();
            baseSource.Overlay(overlay);
            baseSource.destination.ShouldBe(overlay.destination);

            baseSource.logAll.GetValueOrDefault().ShouldBe(false);
            overlay.logAll = true;
            baseSource.Overlay(overlay);
            baseSource.logAll.ShouldBe(true);
        }
    }
}
