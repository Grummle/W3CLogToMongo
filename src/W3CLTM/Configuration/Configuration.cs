using System.Collections.Generic;

namespace IILogReader.Configuration
{
    public class Configuration
    {
        public Configuration()
        {
            sources = new List<source>();
        }

        public string logPath { get; set; }
        public IList<source> sources { get; set; }

        #region Nested type: source

        public class source
        {
            public source()
            {
                templates = new List<string>();
                conversions = new List<conversion>();
                composites = new List<composite>();
                aliases = new List<elementAlias>();
                multiColumnElements = new List<mulitColumnElement>();
                staticElements = new List<staticElement>();
                drop = new List<string>();
            }

            public bool? enabled { get; set; }
            public int? batchSize { get; set; }
            public string id { get; set; }
            public string logDirectory { get; set; }
            public int? entryLimit { get; set; }
            public logDestination destination { get; set; }
            public IList<conversion> conversions { get; set; }
            public IList<composite> composites { get; set; }
            public IList<elementAlias> aliases { get; set; }
            public IList<string> drop { get; set; }
            public IList<mulitColumnElement> multiColumnElements { get; set; }
            public IList<staticElement> staticElements { get; set; }
            public bool? logAll { get; set; }
            public IList<string> templates { get; set; }

            #region Nested type: composite

            public class composite
            {
                public composite()
                {
                    glue = " ";
                }

                public string elementName { get; set; }
                public string[] elements { get; set; }
                public string glue { get; set; }
            }

            #endregion

            #region Nested type: conversion

            public class conversion
            {
                public string elementName { get; set; }
                public string type { get; set; }
            }

            #endregion

            #region Nested type: elementAlias

            public class elementAlias
            {
                public string elementName { get; set; }
                public string alias { get; set; }
            }

            #endregion

            #region Nested type: logDestination

            public class logDestination
            {
                public string mongoConnectionString { get; set; }
                public string mongoDatabase { get; set; }
                public string mongoCollection { get; set; }
                public int batchSize { get; set; }
            }

            #endregion

            #region Nested type: mulitColumnElement

            public class mulitColumnElement
            {
                public string elementName { get; set; }
                public int colSpan { get; set; }
            }

            #endregion

            #region Nested type: staticElement

            public class staticElement
            {
                public string elementName { get; set; }
                public string elementValue { get; set; }
            }

            #endregion
        }

        #endregion
    }
}