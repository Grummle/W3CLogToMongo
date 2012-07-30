using System.Collections.Generic;
using System.IO;
using System.Linq;
using IILogReader;
using IISLogParser.CommandLine;
using NUnit.Framework;
using Shouldly;

namespace IISLogReader.Tests.Integration
{
    [TestFixture]
    public class LogFetcherTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            lf = new LogFetcher();
            path = @"../../Logs/";
        }

        #endregion

        public LogFetcher lf;
        public string path;

        [Test]
        public void Should_Get_Files_Newer_Then_Last()
        {
            IEnumerable<FileInfo> files = lf.GetLogFiles(path, "u_ex12051312.log");

            files.ForEach(x => x.Name.WriteLine());
            files.Count().ShouldBe(3);
            files.Select(x => x.Name).SequenceEqual(new[] {"u_ex12051312.log", "u_ex12051313.log", "u_ex12051314.log"});
        }

        [Test]
        public void Should_Get_Newest_Log_File()
        {
            IEnumerable<FileInfo> files = lf.GetLogFiles(path, null);

            files.Count().ShouldBe(1);
            files.First().Name.ShouldBe("u_ex12051314.log");
        }

        [Test]
        public void Should_Return_All_If_Null()
        {
            IEnumerable<FileInfo> files = lf.GetLogFiles(path, null, true);

            files.Count().ShouldBe(16);
        }

        [Test]
        public void Should_Return_All_If__Not_Existent()
        {
            IEnumerable<FileInfo> files = lf.GetLogFiles(path, "notthere.log", true);

            files.Count().ShouldBe(16);
        }

        [Test]
        public void Should_Return_Lines_From_LogFile()
        {
            IEnumerable<FileInfo> files = lf.GetLogFiles(path, null);

            IEnumerable<string> lines = lf.GetLines(files.First());

            lines.Count().ShouldBe(19833);
        }

        [Test]
        public void Should_Return_Newest_If_Last_Doesnt_Exist()
        {
            IEnumerable<FileInfo> files = lf.GetLogFiles(path, "notthere.log");

            files.Count().ShouldBe(1);
            files.First().Name.ShouldBe("u_ex12051314.log");
        }
    }
}