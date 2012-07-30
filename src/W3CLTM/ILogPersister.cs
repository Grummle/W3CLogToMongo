using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;

namespace IILogReader
{
    public interface ILogPersister
    {
        int BatchSize { get; set; }
        void Persist(IEnumerable<IDictionary<string, object>> entries);
    }

    public class MongoLogPersister : ILogPersister
    {
        private readonly MongoCollection<BsonDocument> _collection;
        private readonly Logger _logger;

        public MongoLogPersister(MongoCollection<BsonDocument> collection, Logger logger)
        {
            _collection = collection;
            _logger = logger;
        }

        public int BatchSize { get; set; }

        public virtual void Persist(IEnumerable<IDictionary<string, object>> entries)
        {
            var docs = entries.Select(x => new BsonDocument(x));

            try
            {
                docs.InBatchesOf(BatchSize,_logger).ForEach(x =>
                                                                {
                                                                    if (x.Any())
                                                                        _collection.InsertBatch(x);
                                                                });
            }
            catch (Exception e)
            {
                _logger.Log("Errored Out Persisting to Mongo",e);
            }
        }
    }

    public class LogPersisterFactory
    {
        private readonly Logger _logger;

        protected LogPersisterFactory(){}

        public LogPersisterFactory(Logger logger)
        {
            _logger = logger;
        }

        public virtual ILogPersister GetPersister(string connectionString, string database, string collectionName)
        {
            var collection = MongoServer.Create(connectionString).GetDatabase(database).GetCollection<BsonDocument>(collectionName);
            return new MongoLogPersister(collection,_logger);
        }
    }
}
