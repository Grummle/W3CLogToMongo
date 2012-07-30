using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IILogReader;
using IILogReader.Configuration;
using IISLogParser.CommandLine;
using NSubstitute;
using NUnit.Framework;
using Newtonsoft.Json;
using Shouldly;

namespace IISLogReader.Tests.Integration
{
    [TestFixture]
    public class LogProcessorTests
    {
        private Configuration.source _source;
        private Logger _logger;
        private LogFetcher _logFetcher;
        private StateRecorder _stateRecorder;
        private LogPersisterFactory _logPersisterFactory;
        private LogProcessor _logProcessor;
        private ILogPersister _logPersister;


        [SetUp]
        public void SetUp()
        {
            _logger = new FakeLogger();
            _logFetcher = new LogFetcher();
            _stateRecorder = new FakeStateRecorder();
            _logPersisterFactory = Substitute.For<LogPersisterFactory>();
            _logPersister = new FakePersister();//Substitute.For<ILogPersister>();
            _logPersisterFactory.GetPersister(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(_logPersister);
            _logProcessor = new LogProcessor(new W3CLogReaderFactory(_logger), _logFetcher, _stateRecorder, _logPersisterFactory,_logger);

            _source = new Configuration.source();
            _source.destination = new Configuration.source.logDestination();
            _source.id = "int.test";
            _source.conversions.Add(new Configuration.source.conversion { elementName = "sc-status", type = "int" });

            _source.logDirectory = "../../Logs/";
            _source.destination.mongoConnectionString = "blarg";
            _source.destination.mongoCollection = "blarg";
            _source.destination.mongoDatabase = "blarg";
        }

        [Test]
        public void Should_Persist_Right_Stuff()
        {
            _logProcessor.Process(_source);
            ((FakePersister)_logPersister).Entries.Count.WriteLine();
            ((FakePersister)_logPersister).Entries.Count.ShouldBe(19829);
        }

        [Test]
        public void Should_Persist_Part_Of_File()
        {
            _stateRecorder[_source.id] = new LogReaderState
            {
                lastFile = "u_ex12051314.log",
                lastLine = 19000,
                fields = new string[] { "date", "time", "s-sitename", "s-computername", "s-ip", "cs-method", "cs-uri-stem", "cs-uri-query", "s-port", "cs-username", "c-ip", "cs-version", "cs(User-Agent)", "cs(Cookie)", "cs(Referer)", "cs-host", "sc-status", "sc-substatus", "sc-win32-status", "sc-bytes", "cs-bytes", "time-taken" }
            };
            _logProcessor.Process(_source);
            _stateRecorder.States.WriteJson();
            ((FakePersister)_logPersister).Entries.Count.WriteLine();
            ((FakePersister)_logPersister).Entries.Count.ShouldBe(833);
        }

        [Test]
        public void Should_Process_Multiple_Files_No_State()
        {
            _source.logAll = true;

            _logProcessor.Process(_source);

            ((FakePersister)_logPersister).Entries.Count.WriteLine();
            ((FakePersister)_logPersister).Entries.First().WriteJson();
            ((FakePersister)_logPersister).CallCount.WriteLine();
            ((FakePersister)_logPersister).CallCount.ShouldBe(16);

        }

        [Test]
        public void Should_Process_Multiple_Files_Bad_State()
        {
            _stateRecorder["int.test"] = new LogReaderState { lastFile = "bs.txt", lastLine = 1 };
            _source.logAll = true;

            _logProcessor.Process(_source);

            ((FakePersister)_logPersister).Entries.Count.WriteLine();
            ((FakePersister)_logPersister).CallCount.WriteLine();
            ((FakePersister)_logPersister).CallCount.ShouldBe(16);

        }


        public class FakePersister : ILogPersister
        {
            public IList<IDictionary<string, object>> Entries { get; set; }
            public int BatchSize { get; set; }
            public int CallCount { get; set; }

            public FakePersister()
            {
                Entries = new List<IDictionary<string, object>>();
            }

            public void Persist(IEnumerable<IDictionary<string, object>> entries)
            {
                entries.ForEach(x => Entries.Add(x));
                CallCount++;
            }
        }

        public class FakeLogger : Logger
        {
            public override void Log(string message)
            {
                message.WriteLine();
            }

            public override void Log(string message, Exception exception)
            {
                message.WriteLine();
                throw new Exception("Should Not Be logging Exception");
            }
        }

        public class FakeStateRecorder : StateRecorder
        {
            public override LogReaderState this[string id]
            {
                get
                {
                    return base[id];
                }
                set
                {
                    States.Where(x => x.id == value.id).ToList().ForEach(x => States.Remove(x));
                    value.id = id;
                    States.Add(value);
                }
            }
        }
    }
}
