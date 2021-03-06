using System.Collections.Generic;
using System.IO;
using System.Linq;
using IILogReader;
using IILogReader.Configuration;

namespace IISLogParser.CommandLine
{
    public class LogProcessor
    {
        private readonly LogFetcher _logFetcher;
        private readonly Logger _logger;
        private readonly LogPersisterFactory _persisterFactory;
        private readonly W3CLogReaderFactory _readerFactory;
        private readonly StateRecorder _stateRecorder;

        public LogProcessor(W3CLogReaderFactory readerFactory, LogFetcher logFetcher, StateRecorder stateRecorder,
                            LogPersisterFactory persisterFactory, Logger logger)
        {
            _readerFactory = readerFactory;
            _logFetcher = logFetcher;
            _stateRecorder = stateRecorder;
            _persisterFactory = persisterFactory;
            _logger = logger;
        }

        public void Process(Configuration.source source)
        {
            ILogPersister logPersister = _persisterFactory.GetPersister(source.destination.mongoConnectionString,
                                                                        source.destination.mongoDatabase,
                                                                        source.destination.mongoCollection);
            logPersister.BatchSize = source.batchSize.GetValueOrDefault();
            LogReaderState state = _stateRecorder[source.id];

            IEnumerable<FileInfo> files = _logFetcher.GetLogFiles(source.logDirectory,
                                                                  state.IsNotNull() ? state.lastFile : null,
                                                                  source.logAll.GetValueOrDefault());

            if (state.IsNotNull())
            {
                if (files.Any(x => x.Name == state.lastFile))
                {
                    W3CLogReader w3c = _readerFactory.CreateLogReader(source);
                    w3c.FromLine(state.lastLine);
                    w3c.WithFields(state.fields);
                    _logger.Log("Begine File:\"{0}\" at Line:{1}".Frmat(files.First().FullName, state.lastLine));
                    logPersister.Persist(w3c.Process(_logFetcher.GetLines(files.First())));
                    _stateRecorder[source.id] = new LogReaderState
                                                    {
                                                        lastFile = files.First().Name,
                                                        lastLine = w3c.LastLine,
                                                        fields = w3c.Fields.ToArray()
                                                    };
                    files = files.Skip(1).ToList();
                }
            }

            files.ForEach(file =>
                              {
                                  W3CLogReader w3c = _readerFactory.CreateLogReader(source);
                                  _logger.Log("Begine File:\"{0}\" from line 0".Frmat(file.FullName));
                                  IEnumerable<IDictionary<string, object>> wtf = w3c.Process(_logFetcher.GetLines(file));
                                  logPersister.Persist(wtf);
                                  _stateRecorder[source.id] = new LogReaderState
                                                                  {
                                                                      lastFile = file.Name,
                                                                      lastLine = w3c.LastLine,
                                                                      fields = w3c.Fields.ToArray()
                                                                  };
                              });
        }
    }
}