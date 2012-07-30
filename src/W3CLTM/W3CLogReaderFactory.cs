using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IILogReader.Configuration
{
    public class W3CLogReaderFactory
    {
        private readonly Logger _logger;

        protected W3CLogReaderFactory() {}

        public W3CLogReaderFactory(Logger logger)
        {
            _logger = logger;
        }

        public virtual W3CLogReader CreateLogReader(Configuration.source source)
        {
            var w3c = new W3CLogReader(_logger);

            w3c.BootStrapFromSource(source);
            
            return w3c;
        }
    }
}
