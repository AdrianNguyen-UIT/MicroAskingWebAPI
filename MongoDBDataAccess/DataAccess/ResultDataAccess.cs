using MongoDBDataAccess.Models;
using MongoDB.Driver;

namespace MongoDBDataAccess.DataAccess
{
    public class ResultDataAccess
    {
        private readonly string _connectionString;
        private const string DatabaseName = "MicroAsking";
        private readonly string CollectionName = "Cached_Answers";

        public ResultDataAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IMongoCollection<T> ConnectToMongo<T>(in string collection)
        {
            var client = new MongoClient(_connectionString);
            var db = client.GetDatabase(DatabaseName);
            return db.GetCollection<T>(collection);
        }

        public async Task<List<ResultDBModel>> GetResultByQuestion(string question)
        {
            var collection = ConnectToMongo<ResultDBModel>(CollectionName);
            var results = await collection.FindAsync(result => result.Question == question);
            return results.ToList();
        }

        public Task CreateResult(ResultDBModel result)
        {
            var collection = ConnectToMongo<ResultDBModel>(CollectionName);
            return collection.InsertOneAsync(result);
        }

        public Task UpdateResult(ResultDBModel result)
        {
            var collection = ConnectToMongo<ResultDBModel>(CollectionName);
            var filter = Builders<ResultDBModel>.Filter.Eq("Id", result.Id);
            return collection.ReplaceOneAsync(filter, result, new ReplaceOptions { IsUpsert = true });
        }
    }
}
