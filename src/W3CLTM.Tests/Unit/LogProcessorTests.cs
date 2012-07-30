using System.Collections.Generic;
using System.IO;
using System.Linq;
using IILogReader;
using IILogReader.Configuration;
using IISLogParser.CommandLine;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace IISLogReader.Tests.Unit
{
    [TestFixture]
    public class LogProcessorTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            persisterFactory = Substitute.For<LogPersisterFactory>();
            logPersister = Substitute.For<ILogPersister>();
            persisterFactory.GetPersister(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(logPersister);
            source = new Configuration.source();
            source.destination = new Configuration.source.logDestination();
            source.destination.mongoCollection = "collection";
            source.destination.mongoConnectionString = "con string";
            source.destination.mongoDatabase = "database";
            source.logDirectory = @"c:\some\path\";
            source.batchSize = 100;
            source.id = "test";
            file = new FileInfo("asdf");
            readerFactory = Substitute.For<W3CLogReaderFactory>();
            logFetcher = Substitute.For<LogFetcher>();
            w3c = Substitute.For<W3CLogReader>();
            w3c.Fields.Returns(new[] {"f1", "f2"});
            sr = Substitute.For<StateRecorder>();

            readerFactory.CreateLogReader(source).Returns(w3c);
            lp = new LogProcessor(readerFactory, logFetcher, sr, persisterFactory, Substitute.For<Logger>());
        }

        #endregion

        public LogProcessor lp;
        public W3CLogReader w3c;
        public Configuration.source source;
        public W3CLogReaderFactory readerFactory;
        public LogFetcher logFetcher;
        public StateRecorder sr;
        public FileInfo file;
        private ILogPersister logPersister;
        public LogPersisterFactory persisterFactory;

        [Test]
        public void Should_Create_Persister()
        {
            var lines = new List<string>();
            var lines2 = new List<string>();
            var lines3 = new List<string>();
            var file1 = new FileInfo("file1");
            var file2 = new FileInfo("file2");
            var file3 = new FileInfo("file3");

            logFetcher.GetLogFiles(source.logDirectory, null).Returns(new[] {file1, file2, file3});
            logFetcher.GetLines(file1).Returns(lines);
            logFetcher.GetLines(file2).Returns(lines2);
            logFetcher.GetLines(file3).Returns(lines3);

            w3c.LastLine.Returns(69);
            var stuff = new[] {"stupid", "fucking", "dictionaries"};

            IEnumerable<Dictionary<string, object>> dic = (new Dictionary<string, object>()).toEnumerable();
            IEnumerable<Dictionary<string, object>> dic2 = (new Dictionary<string, object>()).toEnumerable();
            IEnumerable<Dictionary<string, object>> dic3 = (new Dictionary<string, object>()).toEnumerable();
            w3c.Process(lines).Returns(dic);
            w3c.Process(lines2).Returns(dic2);
            w3c.Process(lines3).Returns(dic3);


            lp.Process(source);

            persisterFactory.Received().GetPersister(source.destination.mongoConnectionString,
                                                     source.destination.mongoDatabase,
                                                     source.destination.mongoCollection);
        }

        [Test]
        public void Should_Persist_Entries_From_Each_File()
        {
            var lines = new List<string>();
            var lines2 = new List<string>();
            var lines3 = new List<string>();
            var file1 = new FileInfo("file1");
            var file2 = new FileInfo("file2");
            var file3 = new FileInfo("file3");

            logFetcher.GetLogFiles(source.logDirectory, null).Returns(new[] {file1, file2, file3});
            logFetcher.GetLines(file1).Returns(lines);
            logFetcher.GetLines(file2).Returns(lines2);
            logFetcher.GetLines(file3).Returns(lines3);

            w3c.LastLine.Returns(69);
            var stuff = new[] {"stupid", "fucking", "dictionaries"};

            IEnumerable<Dictionary<string, object>> dic = (new Dictionary<string, object>()).toEnumerable();
            IEnumerable<Dictionary<string, object>> dic2 = (new Dictionary<string, object>()).toEnumerable();
            IEnumerable<Dictionary<string, object>> dic3 = (new Dictionary<string, object>()).toEnumerable();
            w3c.Process(lines).Returns(dic);
            w3c.Process(lines2).Returns(dic2);
            w3c.Process(lines3).Returns(dic3);


            lp.Process(source);

            w3c.Process(lines).ShouldBe(dic);
            w3c.Process(lines2).ShouldBe(dic2);
            w3c.Process(lines3).ShouldBe(dic3);

            w3c.Received().Process(lines);
            w3c.Received().Process(lines2);
            w3c.Received().Process(lines3);
            logPersister.Received(3).Persist(Arg.Any<IEnumerable<IDictionary<string, object>>>());
            logPersister.Received().BatchSize = 100;
            logPersister.Received().Persist(dic);
            logPersister.Received().Persist(dic2);
            logPersister.Received().Persist(dic3);
            sr["test"].lastFile.ShouldBe(file3.Name);
            sr["test"].lastLine.ShouldBe(69);
        }

        [Test]
        public void Should_Run_W3C_For_Existing_File()
        {
            var lines = new List<string>();
            logFetcher.GetLogFiles(source.logDirectory, "asdf").Returns(file.toEnumerable());
            logFetcher.GetLines(file).Returns(lines);
            var fields = new string[] {};
            sr["test"] = new LogReaderState {lastLine = 666, lastFile = "asdf", fields = fields};

            lp.Process(source);
            w3c.Received().FromLine(666);
            w3c.Received().Process(lines);
            w3c.Received().WithFields(fields);
        }

        [Test]
        public void Should_Run_W3C_For_Multiple_Files()
        {
            var lines = new List<string>();
            var file1 = new FileInfo("file1");
            var file2 = new FileInfo("file2");
            var file3 = new FileInfo("file3");
            w3c.LastLine.Returns(1, 2, 69);
            logFetcher.GetLogFiles(source.logDirectory, null).Returns(new[] {file1, file2, file3});
            logFetcher.GetLines(Arg.Any<FileInfo>()).Returns(lines);


            lp.Process(source);


            w3c.Received(3).Process(lines);
            sr["test"].lastFile.ShouldBe(file3.Name);
            sr["test"].lastLine.ShouldBe(69);
        }

        [Test]
        public void Should_Run_W3C_For_Newest_File()
        {
            var lines = new List<string>();
            logFetcher.GetLogFiles(source.logDirectory, null).Returns(file.toEnumerable());
            logFetcher.GetLines(file).Returns(lines);
            lp.Process(source);

            w3c.Received().Process(lines);
        }

        [Test]
        public void Should_Save_Final_State()
        {
            var lines = new List<string>();

            var files = new[] {new FileInfo("file1"), new FileInfo("file2"), new FileInfo("file3")};

            logFetcher.GetLogFiles(source.logDirectory, files[0].Name).Returns(files);
            logFetcher.GetLines(Arg.Any<FileInfo>()).Returns(lines);

            sr["test"] = new LogReaderState {lastLine = 20, lastFile = files[0].Name, fields = new[] {"f1", "f2`"}};

            w3c.LastLine.Returns(1, 2, 3);

            lp.Process(source);

            sr.Received()["test"] =
                Arg.Is<LogReaderState>(
                    x => x.lastFile == "file3" && x.lastLine == 3 && x.fields.SequenceEqual(new[] {"f1", "f2"}));
            w3c.Received().Process(lines);
        }

        [Test]
        public void Should_Save_Final_State_For_Saved_And_Only_File()
        {
            var lines = new List<string>();

            var files = new[] {new FileInfo("file1")};

            logFetcher.GetLogFiles(source.logDirectory, files[0].Name).Returns(files);
            logFetcher.GetLines(Arg.Any<FileInfo>()).Returns(lines);
            w3c.LastLine.Returns(1);


            //initial state
            sr["test"] = new LogReaderState {lastLine = 20, lastFile = files[0].Name, fields = new[] {"f1", "f2`"}};

            lp.Process(source);
            //final state should be thus
            sr.Received()["test"] =
                Arg.Is<LogReaderState>(
                    x => x.lastFile == "file1" && x.lastLine == 1 && x.fields.SequenceEqual(new[] {"f1", "f2"}));
        }

        [Test]
        public void Should_Save_State()
        {
            var lines = new List<string>();
            logFetcher.GetLogFiles(source.logDirectory, null).Returns(file.toEnumerable());
            logFetcher.GetLines(file).Returns(lines);
            w3c.LastLine.Returns(69);

            lp.Process(source);

            sr.Received()["test"] =
                Arg.Is<LogReaderState>(
                    x => x.lastFile == "asdf" && x.lastLine == 69 && x.fields.SequenceEqual(new[] {"f1", "f2"}));
            w3c.Received().Process(lines);
        }

        [Test]
        public void Should_Set_File_Default_No_State()
        {
            source.logAll = true;
            var lines = new List<string>();
            logFetcher.GetLogFiles(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>()).Returns(file.toEnumerable());
            logFetcher.GetLines(file).Returns(lines);
            lp.Process(source);

            logFetcher.Received().GetLogFiles(Arg.Any<string>(), Arg.Any<string>(), true);
        }

        [Test]
        public void Should_Set_File_Default_With_State()
        {
            source.logAll = true;
            sr["test"] = new LogReaderState {lastFile = file.Name, lastLine = 10, fields = new[] {"f1", "f2"}};
            var lines = new List<string>();
            logFetcher.GetLogFiles(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>()).Returns(file.toEnumerable());
            logFetcher.GetLines(file).Returns(lines);
            lp.Process(source);

            logFetcher.Received().GetLogFiles(Arg.Any<string>(), Arg.Any<string>(), true);
        }

//        [Test]
//        public void Should_Save
    }
}