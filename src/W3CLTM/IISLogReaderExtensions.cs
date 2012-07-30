using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap.TypeRules;

namespace IILogReader
{
    public static class IISLogReaderExtensions
    {
        public static IEnumerable<string> ReadLines(this StreamReader stream)
        {
            using (stream)
            {
                while (!stream.EndOfStream)
                {
                    yield return stream.ReadLine();
                }
            }
        }

        public static void BootStrapFromSource(this W3CLogReader w3c, Configuration.Configuration.source source)
        {
            source.aliases.ForEach(alias => w3c.AliasElement(alias.alias, alias.elementName));
            source.conversions.ForEach(conversion => w3c.Convert(conversion.elementName).ToType(Type.GetType("System." + (conversion.type == "int" ? "Int32" : conversion.type))));
            source.composites.ForEach(composite => w3c.AddComposite.Named(composite.elementName).FromElements(composite.elements));
            source.multiColumnElements.ForEach(mce => w3c.MultiColumnElement(mce.elementName, mce.colSpan));
            source.staticElements.ForEach(staticElement => w3c.AddStaticElement(staticElement.elementName, staticElement.elementValue));
            source.drop.ForEach(drop => w3c.Removals.Add(drop));
            w3c.Limit(source.entryLimit.GetValueOrDefault());
        }

        public static void ApplyTemplate(this Configuration.Configuration.source source, JObject job)
        {
            if (job.Properties().Any(x => x.Name == "logAll"))
                source.logAll = (bool)job["logAll"];

            if (job.Properties().Any(x => x.Name == "batchSize"))
                source.batchSize = (int)job["batchSize"];

            if (job.Properties().Any(x => x.Name == "id"))
                source.id = (string)job["id"];

            if (job.Properties().Any(x => x.Name == "logDirectory"))
                source.logDirectory = (string)job["logDirectory"];

            if (job.Properties().Any(x => x.Name == "enabled"))
                source.enabled = (bool)job["enabled"];

            if (job.Properties().Any(x => x.Name == "conversions"))
                job["conversions"].Children().ForEach(x=>source.conversions.Add(JsonConvert.DeserializeObject<Configuration.Configuration.source.conversion>(x.ToString())));

            if (job.Properties().Any(x => x.Name == "composites"))
                job["composites"].Children().ForEach(x=>source.composites.Add(JsonConvert.DeserializeObject<Configuration.Configuration.source.composite>(x.ToString())));

            if (job.Properties().Any(x => x.Name == "muliColumnElements"))
                job["multiColumnElements"].Children().ForEach(x=>source.multiColumnElements.Add(JsonConvert.DeserializeObject<Configuration.Configuration.source.mulitColumnElement>(x.ToString())));

            if (job.Properties().Any(x => x.Name == "staticElements"))
                job["staticElements"].Children().ForEach(x=>source.staticElements.Add(JsonConvert.DeserializeObject<Configuration.Configuration.source.staticElement>(x.ToString())));

            if (job.Properties().Any(x => x.Name == "aliases"))
                job["aliases"].Children().ForEach(x=>source.aliases.Add(JsonConvert.DeserializeObject<Configuration.Configuration.source.elementAlias>(x.ToString())));

            if (job.Properties().Any(x => x.Name == "templates"))
                job["templates"].Children().ForEach(x=>source.templates.Add(x.ToString()));

        }

        public static void Overlay(this Configuration.Configuration.source baseSource, Configuration.Configuration.source overlay)
        {
            if (overlay.enabled.HasValue)
                baseSource.enabled = overlay.enabled.Value;
            if (overlay.batchSize.HasValue)
                baseSource.batchSize = overlay.batchSize.Value;
            if (overlay.entryLimit.HasValue)
                baseSource.entryLimit = overlay.entryLimit.Value;
            if (overlay.logAll.HasValue)
                baseSource.logAll = overlay.logAll.Value;
            if (overlay.id.IsNotNullOrEmpty())
                baseSource.id = overlay.id;
            if (overlay.logDirectory.IsNotNullOrEmpty())
                baseSource.logDirectory = overlay.logDirectory;
            if (overlay.destination.IsNotNull())
                baseSource.destination = overlay.destination;
            overlay.conversions.ForEach(x=>baseSource.conversions.Add(x));
            overlay.multiColumnElements.ForEach(x=>baseSource.multiColumnElements.Add(x));
            overlay.composites.ForEach(x=>baseSource.composites.Add(x));
            overlay.aliases.ForEach(x=>baseSource.aliases.Add(x));
            overlay.staticElements.ForEach(x=>baseSource.staticElements.Add(x));
            overlay.templates.ForEach(x=>baseSource.templates.Add(x));
            overlay.drop.ForEach(x=>baseSource.drop.Add(x));
        }
        
        public static void WriteLine(this string value, params object[] stuff)
        {
            Console.WriteLine(value.Frmat(stuff));
        }

        public static string Frmat(this string value, params object[] stuff)
        {
            var temp = value;

            stuff.ForEach((x, y) => temp = temp.Replace("{" + y + "}", x.ToString()));

            return temp;
        }

        public static void WriteJson(this object value)
        {
            Console.WriteLine(JsonConvert.SerializeObject(value, Formatting.Indented));
        }

        public static void WriteLine(this object value, params object[] stuff)
        {
            Console.WriteLine(value.ToString().Frmat(stuff));
        }

        public static char[] Delimiters = new char[] { ' ', '\t' };

        public static bool IsFieldsDirective(this string line)
        {
            return line.Substring(0, 8).ToLower().Equals("#fields:");
        }

        public static bool IsDirective(this string line)
        {
            return line.Substring(0, 1).Equals("#");
        }

        public static IEnumerable<string> SplitFields(this string line)
        {
            return line.Substring(line.IndexOf(':') + 1).Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);
        }

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }

        public static Configuration.Configuration.source ProcessSource(this Configuration.Configuration.source initialSource)
        {
            var sourceBase = new Configuration.Configuration.source();

            if(initialSource.templates.Any())
                initialSource.templates.ForEach(x=>sourceBase.Overlay(JsonConvert.DeserializeObject<Configuration.Configuration.source>(File.ReadAllText(x))));
            sourceBase.Overlay(initialSource);

            return sourceBase;
        }

        public static bool IsNull(this object value)
        {
            return value == null;
        }

        public static bool IsNotNull(this object value)
        {
            return value != null;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> value)
        {
            return value == null || !value.Any();
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNotNullOrEmpty(this string value)
        {
            return !value.IsNullOrEmpty();
        }

        public static string Join(this IEnumerable<object> source, string seperator)
        {
            return string.Join(seperator, source.Select(x => x.ToString()));
        }

        public static void ForEach<T>(this IEnumerable<T> ie, Action<T, int> action)
        {
            var i = 0;
            foreach (var e in ie) action(e, i++);
        }

        public static IEnumerable<string> SplitValues(this string line)
        {
            //@"""(?<value>.+?)""|(?<value>[^\s""]+)"
            //@"\s?""(?<value>.+?)""\s|(?<value>[^\s]+)"
            Regex regexObj = new Regex(@"\s?""(?<value>.+?)""\s|(?<value>[^\s]+)");
            var matchResults = regexObj.Matches(line);
            return matchResults.Cast<Match>().Select(x => x.Groups["value"].Value);
        }

        public static IEnumerable<T> toEnumerable<T>(this T source)
        {
            return new[] { source };
        }

        public static IEnumerable<IEnumerable<T>> InBatchesOf<T>(this IEnumerable<T> source, int max)
        {
            if (max == 0)
                yield return source;
            else
            {
                List<T> toReturn = new List<T>(max);
                foreach (var item in source)
                {
                    toReturn.Add(item);
                    if (toReturn.Count == max)
                    {
                        yield return toReturn;
                        toReturn = new List<T>(max);
                    }
                }
                if (toReturn.Any())
                {
                    yield return toReturn;
                }
            }
        }

        public static IEnumerable<IEnumerable<T>> InBatchesOf<T>(this IEnumerable<T> source, int max, Logger logger)
        {
            var sw = new System.Diagnostics.Stopwatch();
            if (max == 0)
            {
                yield return source;
            }
            else
            {
                List<T> toReturn = new List<T>(max);
                sw.Start();
                foreach (var item in source)
                {
                    toReturn.Add(item);
                    if (toReturn.Count == max)
                    {
                        sw.Stop();
                        logger.Log("{0} elements in {1}ms. {2}p/s".Frmat(max, sw.ElapsedMilliseconds, sw.ElapsedMilliseconds > 0 ? (max / sw.ElapsedMilliseconds) * 1000L : 0L));
                        yield return toReturn;
                        toReturn = new List<T>(max);
                    }
                }
                if (toReturn.Any())
                {
                    logger.Log("{0} elements in {1}ms. {2}p/s".Frmat(toReturn.Count, sw.ElapsedMilliseconds, sw.ElapsedMilliseconds > 0 ? (toReturn.Count/ sw.ElapsedMilliseconds) * 1000L : 0L));
                    yield return toReturn;
                }
            }
        }

    }
}