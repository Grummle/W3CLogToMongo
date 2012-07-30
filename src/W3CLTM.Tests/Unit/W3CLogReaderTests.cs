using System;
using System.Collections.Generic;
using System.Linq;
using IILogReader;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace IISLogReader.Tests.Unit
{
    [TestFixture]
    public class W3CLogReaderTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            file =
                @"#Version: 1.0
#Date: 12-Jan-1996 00:00:00
#Fields: time cs-method cs-uri
00:34:23 GET /foo/bar.html
12:21:16 GET /foo/bar.html
12:45:52 GET /foo/bar.html
12:57:34 GET /foo/bar.html";

            file2 =
                @"#Version: 1.0
#Date: 12-Jan-1996 00:00:00
#Fields: date time cs-method cs-uri
2012-07-23 00:34:23 GET /foo/bar.html
2012-07-23 12:21:16 GET /foo/bar.html
2012-07-23 12:45:52 GET /foo/bar.html
2012-07-23 12:57:34 GET /foo/bar.html";

            fields = new[] {"time", "cs-method", "cs-uri"};
            values = new List<string[]>();
            values.Add(new[] {"00:34:23", "GET", "/foo/bar.html"});
            values.Add(new[] {"12:21:16", "GET", "/foo/bar.html"});
            values.Add(new[] {"12:45:52", "GET", "/foo/bar.html"});
            values.Add(new[] {"12:57:34", "GET", "/foo/bar.html"});
            values.Add(new[] {"2012-07-23", "00:34:23", "GET", "/foo/bar.html"});
            values.Add(new[] {"2012-07-23", "12:21:16", "GET", "/foo/bar.html"});
            values.Add(new[] {"2012-07-23", "12:45:52", "GET", "/foo/bar.html"});
            values.Add(new[] {"2012-07-23", "12:57:34", "GET", "/foo/bar.html"});

            logger = Substitute.For<Logger>();

            logger.When(x => x.Log(Arg.Any<string>(), Arg.Any<Exception>()))
                .Do(x =>
                        {
                            x.Args()[0].WriteLine();
                            throw new Exception("Shouldn't be throwing exceptions:{0}".Frmat(x.Args()[0]),
                                                (Exception) x.Args()[1]);
                        });
            w3cReader = new W3CLogReader(logger);
        }

        #endregion

        public string file;
        public string file2;
        public string[] fields;
        public IList<string[]> values;
        public W3CLogReader w3cReader;
        public Logger logger;

        [TestCase("#Fields: time cs-method cs-uri", TestName = "Standard Field")]
        [TestCase("#fields: time cs-method cs-uri", TestName = "Case Insensitive Field")]
        public void Should_Identify_Fields_Line(string line)
        {
            line.IsFieldsDirective().ShouldBe(true);
        }

        [TestCase("#Fields: time cs-method cs-uri", TestName = "Space delimiter")]
        [TestCase("#Fields:time\tcs-method\tcs-uri", TestName = "Tab delimiter")]
        [TestCase("#Fields: time\t cs-method\tcs-uri ", TestName = "Ignore Whitespace")]
        public void Should_Return_Fields(string line)
        {
            line.SplitFields().ForEach(x => Console.Write(x + " "));
            line.SplitFields().SequenceEqual(new[] {"time", "cs-method", "cs-uri"}).ShouldBe(true);
        }

        [TestCase("00:34:23 GET /foo/bar.html", TestName = "Space Delimited")]
        [TestCase("00:34:23 \"GET\" /foo/bar.html", TestName = "Qouted")]
        [TestCase("00:34:23\tGET\t/foo/bar.html", TestName = "Tab Delimited")]
        [TestCase(" 00:34:23 \tGET \t/foo/bar.html", TestName = "Ignore Whitespace")]
        public void Should_Return_Field_Values(string line)
        {
            line.SplitValues().ForEach(x => Console.Write(x + " "));
            line.SplitValues().SequenceEqual(new[] {"00:34:23", "GET", "/foo/bar.html"}).ShouldBe(true);
        }

        [TestCase("\"hello newman\" \"you bastard, I'll kill you\"    die!", TestName = "Ignore Whitespace")]
        public void Should_Return_Field_Values_EnclosedSpaces(string line)
        {
            line.SplitValues().ForEach(x => Console.Write(x + " "));
            line.SplitValues().SequenceEqual(new[] {"hello newman", "you bastard, I'll kill you", "die!"}).ShouldBe(true);
        }

        public void Should_Combine_Columns_To_New_Column()
        {
            var fields = new[] {"date", "time", "method", "status"};
            IEnumerable<string> lines = new[] {"2012-07-23 09:57:31 GET 400"};
            w3cReader.WithFields(fields);

            w3cReader.AddComposite.FromElements("date", "time").Named("LogTime");

            List<IDictionary<string, object>> entries = w3cReader.Process(lines).ToList();
            entries.First().ForEach(y => { Console.WriteLine("{0} {1}", y.Key, y.Value); });
            entries.First().ForEach(x => Console.WriteLine("{0} {1}", x.Key, x.Value));
            entries.First()["LogTime"].ShouldBe("2012-07-23 09:57:31");
        }

        [TestCase("status", true, typeof (int), TestName = "Element Exists")]
        [TestCase("farkle", false, typeof (DateTime), TestName = "Element Does Not Exist")]
        [TestCase("blarg", false, typeof (DateTime), TestName = "Element Is Empty")]
        public void Should_Convert_Column_To_Type(string elementName, bool exists, Type dataType)
        {
            var fields = new[] {"date", "time", "method", "status", "blarg"};
            IEnumerable<string> lines = new[] {"2012-07-23 09:57:31 GET 400 -"};
            w3cReader.WithFields(fields);

            w3cReader.Convert(elementName).ToType(dataType);

            List<IDictionary<string, object>> entries = w3cReader.Process(lines).ToList();

            entries.First().ForEach(x => Console.WriteLine("{0} {1}", x.Key, x.Value));
            if (entries.First().ContainsKey(elementName))
                entries.First()[elementName].GetType().ShouldBe(dataType);
        }

        [TestCase("method", TestName = "Element Exists")]
        [TestCase("flatulance", TestName = "Element Doesn't Exist")]
        public void Should_Remove_Specified_Elements(string elementName)
        {
            var fields = new[] {"date", "time", "method", "status"};
            IEnumerable<string> lines = new[] {"2012-07-23 09:57:31 GET 400"};
            w3cReader.WithFields(fields);

            w3cReader.RemoveElement(elementName);
            List<IDictionary<string, object>> entries = w3cReader.Process(lines).ToList();

            entries.First().ForEach(x => Console.WriteLine("{0} {1}", x.Key, x.Value));
            entries.First().ContainsKey(elementName).ShouldBe(false);
        }

        [Test]
        public void Should_Add_Specified_Static_Elements()
        {
            var fields = new[] {"date", "time", "method", "status"};
            IEnumerable<string> lines = new[] {"2012-07-23 09:57:31 GET 400"};
            w3cReader.WithFields(fields);
            w3cReader.AddStaticElement("static", "some shit");

            List<IDictionary<string, object>> entries = w3cReader.Process(lines).ToList();

            entries.First().ForEach(x => Console.WriteLine("{0} {1}", x.Key, x.Value));
            entries.First().Count.ShouldBe(5);
            entries.First()["static"].ShouldBe("some shit");
        }

        [Test]
        public void Should_Alias_Field_Names()
        {
            var fields = new[] {"date", "time", "method", "status"};
            IEnumerable<string> lines = new[] {"2012-07-23 09:57:31 GET 400"};
            w3cReader.WithFields(fields);

            w3cReader.AliasElement("method", "fargo");
            List<IDictionary<string, object>> entries = w3cReader.Process(lines).ToList();

            entries.First().ForEach(x => Console.WriteLine("{0} {1}", x.Key, x.Value));
            entries.First().Count.ShouldBe(4);
            entries.First().ContainsKey("method").ShouldBe(false);
            entries.First().ContainsKey("fargo").ShouldBe(true);
            entries.First()["fargo"].ShouldBe("GET");
        }

        [Test]
        public void Should_Combine_Columns_To_New_Column_And_Convert_To_Type()
        {
            var fields = new[] {"date", "time", "method", "status"};
            IEnumerable<string> lines = new[] {"2012-07-23 09:57:31 GET 400"};
            w3cReader.WithFields(fields);

            w3cReader.AddComposite.FromElements("date", "time").Named("LogTime");
            w3cReader.Convert("LogTime").ToType(typeof (DateTime));

            List<IDictionary<string, object>> entries = w3cReader.Process(lines).ToList();
            entries.First().ForEach(y => Console.WriteLine("{0} {1}", y.Key, y.Value));
            entries.First().ForEach(x => Console.WriteLine("{0} {1}", x.Key, x.Value));
            entries.First()["LogTime"].GetType().ShouldBe(typeof (DateTime));
        }

        [Test]
        public void Should_Combine_Columns_To_New_Column_Using_Specified_Glue()
        {
            var fields = new[] {"date", "time", "method", "status"};
            IEnumerable<string> lines = new[] {"2012-07-23 09:57:31 GET 400"};
            w3cReader.WithFields(fields);

            w3cReader.AddComposite.FromElements("date", "time").Named("LogTime").JoinedWith("Biznatch");

            List<IDictionary<string, object>> entries = w3cReader.Process(lines).ToList();

            entries.First().ForEach(x => Console.WriteLine("{0} {1}", x.Key, x.Value));
            entries.First()["LogTime"].ShouldBe("2012-07-23Biznatch09:57:31");
        }

        [Test]
        public void Should_Find_Fields_Directive()
        {
            IEnumerable<string> lines = file.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            w3cReader.Process(lines);
            Enumerable.SequenceEqual(w3cReader.Fields, new[] {"time", "cs-method", "cs-uri"});
        }

        [Test, ExpectedException(typeof (Exception), UserMessage = "No Fields Found Before Log Entries")]
        public void Should_Give_Infromative_Erorr_if_Fields_Not_Supplied()
        {
            IEnumerable<string> lines = new[] {"2012-07-23 09:57:31 GET 400"};
            w3cReader.Process(lines).ToList();
        }

        [Test]
        public void Should_Handle_Conversion_Fail_Gracefully()
        {
            //This one throws an Exception, just not any unhandled ones.
            w3cReader = new W3CLogReader(Substitute.For<Logger>());
            var fields = new[] {"date", "time", "method", "status"};
            IEnumerable<string> lines = new[] {"2012-07-23 09:57:31 - booger"};
            w3cReader.Convert("status").ToType(typeof (int));
            w3cReader.WithFields(fields);

            List<IDictionary<string, object>> entries = w3cReader.Process(lines).ToList();
        }

        [Test]
        public void Should_Handle_Empty_Lame_MS_Multi_Column_Nonsense()
        {
            var fields = new[] {"date", "Endtime-UTC", "method", "status"};
            IEnumerable<string> lines = new[] {"2012-07-23 - GET 400"};
            w3cReader.WithFields(fields);

            w3cReader.MultiColumnElement("Endtime-UTC", 2);
            List<IDictionary<string, object>> entries = w3cReader.Process(lines).ToList();

            entries.First().ForEach(x => Console.WriteLine("{0} {1}", x.Key, x.Value));
            entries.First().Count.ShouldBe(3);
            entries.First()["date"].ShouldBe("2012-07-23");
            entries.First()["method"].ShouldBe("GET");
            entries.First()["status"].ShouldBe("400");
        }

        [Test]
        public void Should_Handle_Lame_MS_Multi_Column_Nonsense()
        {
            var fields = new[] {"date", "Endtime-UTC", "method", "status"};
            IEnumerable<string> lines = new[] {"2012-07-23 2012-07-23 09:57:31 GET 400"};
            w3cReader.WithFields(fields);

            w3cReader.MultiColumnElement("Endtime-UTC", 2);
            List<IDictionary<string, object>> entries = w3cReader.Process(lines).ToList();

            entries.First().ForEach(x => Console.WriteLine("{0} {1}", x.Key, x.Value));
            entries.First().Count.ShouldBe(4);
            entries.First()["Endtime-UTC"].ShouldBe("2012-07-23 09:57:31");
        }

        [Test]
        public void Should_Handle_Qoutes_In_Middle_Of_Content()
        {
            string line =
                "\"2012-05-13\" 03:40:14 W3SVC2 RMWEB2 172.16.1.20 POST \"/libraries/form_wizard/process_subscribe\".asp\" - 80 - 114.216.237.9 HTTP/1.1 \"Mozilla/5.0+(compatible;+MSIE+9.0;+Windows+NT+6.1;+Trident/5.0)\" - http://executiveeducation.wharton.upenn.edu/_landingpages/chinese-landing-page.cfm?slx=GooglePC&WT.srch=1&source=gsweeaa170&kw=\"%E9%A2%86%E5%AF%BC%E5%8A%9B\"&creative={creatives} go.reachmail.net 302 0 0 520";
            line.SplitValues().Count().ShouldBe(20);
        }

//        [TestCase("00:34:23 GET /foo/bar.html", TestName = "Matching Fields and Values")]
//        [TestCase("00:34:23 GET /foo/bar.html monkeyWrench", TestName = "Non Matching Fields and Values")]
//        public void Should_Return_Entry(string line)
//        {
//            IEnumerable<string> fields = new List<string> { "time", "cs-method", "cs-uri" };
//            var expected = new Dictionary<string, string>();
//            expected.Add("time", "00:34:23");
//            expected.Add("cs-method", "GET");
//            expected.Add("cs-uri", "/foo/bar.html");
//            new ListDictionary();
////            CollectionAssert.AreEqual(expected, line.BuildEntry(fields));
//        }

        [Test]
        public void Should_Identify_Directive()
        {
            "#Some Directive".IsDirective().ShouldBe(true);
        }

        [Test]
        public void Should_Not_Be_Fields_Line()
        {
            string line = "some random crap";
            line.IsFieldsDirective().ShouldBe(false);
        }

        [Test]
        public void Should_Not_Identify_Directive()
        {
            "Some values".IsDirective().ShouldBe(false);
        }

        [Test]
        public void Should_Not_Make_Composite_When_Elements_Missing()
        {
            var fields = new[] {"date", "time", "method", "status"};
            IEnumerable<string> lines = new[] {"2012-07-23 09:57:31 GET 400"};
            w3cReader.WithFields(fields);

            w3cReader.AddComposite.FromElements("date", "time", "ua").Named("LogTime");

            List<IDictionary<string, object>> entries = w3cReader.Process(lines).ToList();

            entries.First().ForEach(x => Console.WriteLine("{0} {1}", x.Key, x.Value));
            entries.First().ContainsKey("LogTime");
        }

        [Test]
        public void Should_Process_2_Entries()
        {
            IEnumerable<string> lines = file.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            List<IDictionary<string, object>> entries = w3cReader.Limit(2).Process(lines).ToList();
            var expextedEntries = new List<IDictionary<string, string>>();
            var dic1 = new Dictionary<string, string>();
            fields.ForEach((x, y) => dic1.Add(x, values[0][y]));
            expextedEntries.Add(dic1);
            var dic2 = new Dictionary<string, string>();
            fields.ForEach((x, y) => dic2.Add(x, values[1][y]));
            expextedEntries.Add(dic2);
            var dic3 = new Dictionary<string, string>();
            fields.ForEach((x, y) => dic3.Add(x, values[2][y]));
            expextedEntries.Add(dic3);
            var dic4 = new Dictionary<string, string>();
            fields.ForEach((x, y) => dic4.Add(x, values[3][y]));
            expextedEntries.Add(dic4);

            expextedEntries.ForEach((x, i) =>
                                        {
                                            Console.WriteLine(i);
                                            x.ForEach(y => { Console.WriteLine("{0} {1}", y.Key, y.Value); });
                                            Console.WriteLine();
                                        });
            entries.ForEach((x, i) =>
                                {
                                    Console.WriteLine(i);
                                    x.ForEach(y => { Console.WriteLine("{0} {1}", y.Key, y.Value); });
                                    Console.WriteLine();
                                });
            entries.Count.ShouldBe(2);
            entries.ForEach((x, y) =>
                                {
                                    x.Count.ShouldBe(expextedEntries[y].Count);
                                    x.ForEach(z => z.Value.ShouldBe(expextedEntries[y][z.Key]));
                                });
        }

        [Test]
        public void Should_Process_Entries()
        {
            IEnumerable<string> lines = file.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            List<IDictionary<string, object>> entries = w3cReader.Process(lines).ToList();
            var expextedEntries = new List<IDictionary<string, object>>();
            var dic1 = new Dictionary<string, object>();
            fields.ForEach((x, y) => dic1.Add(x, values[0][y]));
            expextedEntries.Add(dic1);
            var dic2 = new Dictionary<string, object>();
            fields.ForEach((x, y) => dic2.Add(x, values[1][y]));
            expextedEntries.Add(dic2);
            var dic3 = new Dictionary<string, object>();
            fields.ForEach((x, y) => dic3.Add(x, values[2][y]));
            expextedEntries.Add(dic3);
            var dic4 = new Dictionary<string, object>();
            fields.ForEach((x, y) => dic4.Add(x, values[3][y]));
            expextedEntries.Add(dic4);

            expextedEntries.ForEach((x, i) =>
                                        {
                                            Console.WriteLine(i);
                                            x.ForEach(y => { Console.WriteLine("{0} {1}", y.Key, y.Value); });
                                            Console.WriteLine();
                                        });
            entries.ForEach((x, i) =>
                                {
                                    Console.WriteLine(i);
                                    x.ForEach(y => { Console.WriteLine("{0} {1}", y.Key, y.Value); });
                                    Console.WriteLine();
                                });
            entries.ShouldNotBeEmpty();
            entries.ForEach((x, y) => x.SequenceEqual(expextedEntries[y]).ShouldBe(true));
        }

        [Test]
        public void Should_Process_Entries_WithMultiple_Field_Directives()
        {
            IEnumerable<string> lines = (file + Environment.NewLine + file2).Split(new[] {Environment.NewLine},
                                                                                   StringSplitOptions.RemoveEmptyEntries);
            List<IDictionary<string, object>> entries = w3cReader.Process(lines).ToList();
            var expextedEntries = new List<IDictionary<string, object>>();
            var dic1 = new Dictionary<string, object>();
            fields.ForEach((x, y) => dic1.Add(x, values[0][y]));
            expextedEntries.Add(dic1);
            var dic2 = new Dictionary<string, object>();
            fields.ForEach((x, y) => dic2.Add(x, values[1][y]));
            expextedEntries.Add(dic2);
            var dic3 = new Dictionary<string, object>();
            fields.ForEach((x, y) => dic3.Add(x, values[2][y]));
            expextedEntries.Add(dic3);
            var dic4 = new Dictionary<string, object>();
            fields.ForEach((x, y) => dic4.Add(x, values[3][y]));
            expextedEntries.Add(dic4);
            IEnumerable<string> fields2 = (new[] {"date"}).Union(fields);
            var dic5 = new Dictionary<string, object>();
            fields2.ForEach((x, y) => dic5.Add(x, values[4][y]));
            expextedEntries.Add(dic5);
            var dic6 = new Dictionary<string, object>();
            fields2.ForEach((x, y) => dic6.Add(x, values[5][y]));
            expextedEntries.Add(dic6);
            var dic7 = new Dictionary<string, object>();
            fields2.ForEach((x, y) => dic7.Add(x, values[6][y]));
            expextedEntries.Add(dic7);
            var dic8 = new Dictionary<string, object>();
            fields2.ForEach((x, y) => dic8.Add(x, values[7][y]));
            expextedEntries.Add(dic8);

            expextedEntries.ForEach((x, i) =>
                                        {
                                            Console.WriteLine(i);
                                            x.ForEach(y => { Console.WriteLine("{0} {1}", y.Key, y.Value); });
                                            Console.WriteLine();
                                        });
            entries.ForEach((x, i) =>
                                {
                                    Console.WriteLine(i);
                                    x.ForEach(y => { Console.WriteLine("{0} {1}", y.Key, y.Value); });
                                    Console.WriteLine();
                                });
            entries.Count.ShouldBe(8);
            entries.ForEach((x, y) => x.SequenceEqual(expextedEntries[y]).ShouldBe(true));
        }

        [Test]
        public void Should_Process_From_Given_LineNumber()
        {
            IEnumerable<string> fields2 = (new[] {"date"}).Union(fields);
            IEnumerable<string> lines = (file + Environment.NewLine + file2).Split(new[] {Environment.NewLine},
                                                                                   StringSplitOptions.RemoveEmptyEntries);
            List<IDictionary<string, object>> entries =
                w3cReader.FromLine(10).WithFields(fields2).Process(lines).ToList();
            var expextedEntries = new List<IDictionary<string, string>>();

            var dic5 = new Dictionary<string, string>();
            fields2.ForEach((x, y) => dic5.Add(x, values[4][y]));
            expextedEntries.Add(dic5);
            var dic6 = new Dictionary<string, string>();
            fields2.ForEach((x, y) => dic6.Add(x, values[5][y]));
            expextedEntries.Add(dic6);
            var dic7 = new Dictionary<string, string>();
            fields2.ForEach((x, y) => dic7.Add(x, values[6][y]));
            expextedEntries.Add(dic7);
            var dic8 = new Dictionary<string, string>();
            fields2.ForEach((x, y) => dic8.Add(x, values[7][y]));
            expextedEntries.Add(dic8);

            expextedEntries.ForEach((x, i) =>
                                        {
                                            Console.WriteLine(i);
                                            x.ForEach(y => { Console.WriteLine("{0} {1}", y.Key, y.Value); });
                                            Console.WriteLine();
                                        });
            entries.ForEach((x, i) =>
                                {
                                    Console.WriteLine(i);
                                    x.ForEach(y => { Console.WriteLine("{0} {1}", y.Key, y.Value); });
                                    Console.WriteLine();
                                });
            entries.Count.ShouldBe(4);
            entries.ForEach((x, y) =>
                                {
                                    x.Count.ShouldBe(expextedEntries[y].Count);
                                    x.ForEach(z => z.Value.ShouldBe(expextedEntries[y][z.Key]));
                                });
        }

        [Test]
        public void Should_Remember_Line_Position()
        {
            IEnumerable<string> lines = file.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            w3cReader.Process(lines).ToList();
            w3cReader.LastLine.ShouldBe(7);
        }

        [Test]
        public void Should_Remember_Line_Position_When_Starting_ElseWhere()
        {
            IEnumerable<string> fields2 = (new[] {"date"}).Union(fields);
            IEnumerable<string> lines = (file + Environment.NewLine + file2).Split(new[] {Environment.NewLine},
                                                                                   StringSplitOptions.RemoveEmptyEntries);
            List<IDictionary<string, object>> entries =
                w3cReader.FromLine(10).WithFields(fields2).Process(lines).ToList();

            entries.Count.ShouldBe(4);
            w3cReader.LastLine.ShouldBe(14);
        }

        [Test]
        public void Should_Remove_Empty_Columns()
        {
            var fields = new[] {"date", "time", "method", "status"};
            IEnumerable<string> lines = new[] {"2012-07-23 09:57:31 - 400"};
            w3cReader.WithFields(fields);

            List<IDictionary<string, object>> entries = w3cReader.Process(lines).ToList();

            entries.First().ForEach(x => Console.WriteLine("{0} {1}", x.Key, x.Value));
            entries.First().Count.ShouldBe(3);
            entries.First().ContainsKey("method").ShouldBe(false);
        }
    }
}