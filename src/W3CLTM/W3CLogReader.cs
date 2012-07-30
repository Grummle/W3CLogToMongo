using System;
using System.Collections.Generic;
using System.Linq;
using IILogReader.Binders;
using Newtonsoft.Json;

namespace IILogReader
{
    public class W3CLogReader
    {
        private readonly Logger _logger;
        protected IEnumerable<IBinder> Binders = PrimitiveBinders.Binders;

        public W3CLogReader()
        {
        }

        public W3CLogReader(Logger logger)
        {
            _logger = logger;
            Fields = new string[] {};
            Composites = new List<CompositeElement>();
            Conversions = new List<Conversion>();
            StaticElements = new List<Tuple<string, string>>();
            Removals = new List<string>();
            Aliases = new Dictionary<string, string>();
            LastLine = 0;
            MultiColumnElements = new Dictionary<string, int>();
        }

        public virtual IEnumerable<string> Fields { get; protected set; }
        public virtual int LastLine { get; protected set; }
        public virtual int EntryLimit { get; set; }
        public virtual IList<CompositeElement> Composites { get; set; }
        public virtual IList<Conversion> Conversions { get; protected set; }
        public virtual IList<Tuple<string, string>> StaticElements { get; set; }
        public virtual IList<string> Removals { get; set; }
        public virtual IDictionary<string, string> Aliases { get; protected set; }
        public virtual IDictionary<string, int> MultiColumnElements { get; set; }

        public CompositeElement AddComposite
        {
            get
            {
                var tempComp = new CompositeElement();
                Composites.Add(tempComp);
                return tempComp;
            }
        }

        public virtual IEnumerable<IDictionary<string, object>> Process(IEnumerable<string> lines)
        {
            int processedLines = 0;
            IDictionary<string, object> entry;

            IEnumerable<string> filteredLines = lines.Skip(LastLine);
            if (EntryLimit != 0)
                filteredLines = filteredLines.TakeWhile(x => processedLines < EntryLimit);
            ApplyAliases();
            foreach (string line in filteredLines)
            {
                LastLine++;
                if (line.IsFieldsDirective())
                {
                    Fields = line.SplitFields();
                    ApplyAliases();
                }

                //Line is an entry
                if (!line.IsDirective())
                {
                    if (Fields.IsNullOrEmpty())
                        throw new Exception("No Fields Found Before Log Entries");
                    entry = BuildEntry(line);
                    try
                    {
                        RemoveEmpties(entry);
                        ProcessStaticElements(entry);
                        BuildComposites(entry);
                        ProcessConversions(entry);
                        processedLines++;
                        Removals.ForEach(x => entry.Remove(x));
                    }
                    catch (Exception e)
                    {
                        _logger.Log(
                            "Error: LogLine:{0}\r\nLine Content:{1}\r\n{2}".Frmat(LastLine, line,
                                                                                  JsonConvert.SerializeObject(entry)), e);
                    }
                    yield return entry;
                    //Entries.Add(entry);
                }
            }
        }

        public void RemoveEmpties(IDictionary<string, object> entry)
        {
            entry.Where(x => x.Value.ToString() == "-" || x.Value.ToString().IsNullOrEmpty()).ToList().ForEach(
                x => entry.Remove(x));
        }

        public IDictionary<string, object> BuildEntry(string line)
        {
            IEnumerator<string> values = line.SplitValues().GetEnumerator();
            var dic = new Dictionary<string, object>();
            values.MoveNext();
            Fields.ForEach(x =>
                               {
                                   string value = values.Current;
                                   if (MultiColumnElements.ContainsKey(x) && (value.IsNullOrEmpty() || value != "-"))
                                   {
                                       for (int index = MultiColumnElements[x] - 1; index > 0; index--)
                                       {
                                           values.MoveNext();
                                           value = string.Join(" ", value, values.Current);
                                       }
                                   }
                                   dic.Add(x, value);
                                   values.MoveNext();
                               });
            return dic;
        }

        public void AliasElement(string elementName, string alias)
        {
            Aliases.Add(elementName, alias);
        }

        public void ApplyAliases()
        {
            Fields = Fields.Select(field => Aliases.ContainsKey(field) ? Aliases[field] : field).ToArray();
        }

        private void ProcessStaticElements(IDictionary<string, object> entry)
        {
            StaticElements.ForEach(x => entry.Add(x.Item1, x.Item2));
        }

        private void ProcessConversions(IDictionary<string, object> entry)
        {
            Conversions.Where(x => entry.ContainsKey(x.ElementName)).ForEach(conversion =>
                                                                                 {
                                                                                     string element =
                                                                                         entry[conversion.ElementName].
                                                                                             ToString();
                                                                                     entry.Remove(conversion.ElementName);
                                                                                     try
                                                                                     {
                                                                                         entry.Add(
                                                                                             conversion.ElementName,
                                                                                             Binders.First(
                                                                                                 x =>
                                                                                                 x.Matches(
                                                                                                     conversion.DataType))
                                                                                                 .Bind(
                                                                                                     conversion.DataType,
                                                                                                     element));
                                                                                     }
                                                                                     catch (Exception e)
                                                                                     {
                                                                                         throw new Exception(
                                                                                             "Conversion failed for value:{0} to {1} in element {2}"
                                                                                                 .Frmat(element,
                                                                                                        conversion.
                                                                                                            DataType.
                                                                                                            Name,
                                                                                                        conversion.
                                                                                                            ElementName),
                                                                                             e);
                                                                                     }
                                                                                 });
        }

        protected void BuildComposites(IDictionary<string, object> entry)
        {
            Composites.Where(x => x.Components.All(y => entry.ContainsKey(y)))
                .ForEach(
                    compositeSpecification =>
                    entry.Add(compositeSpecification.Name, compositeSpecification.BuildComposite(entry)));
        }

        public W3CLogReader FromLine(int line)
        {
            LastLine = line;
            return this;
        }

        public W3CLogReader WithFields(IEnumerable<string> fields)
        {
            Fields = fields;
            return this;
        }

        public W3CLogReader Limit(int i)
        {
            EntryLimit = i;
            return this;
        }

        public Conversion Convert(string elementName)
        {
            var tempConversion = new Conversion {ElementName = elementName};
            Conversions.Add(tempConversion);
            return tempConversion;
        }

        public void AddStaticElement(string name, string value)
        {
            StaticElements.Add(new Tuple<string, string>(name, value));
        }

        public void RemoveElement(string elementName)
        {
            Removals.Add(elementName);
        }

        public void MultiColumnElement(string name, int columnCount)
        {
            MultiColumnElements.Add(name, columnCount);
        }
    }
}