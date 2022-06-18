using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDBDataAccess.Models
{
    public class AnswerDBModel
    {
        public string Answer { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
    }

    public class ResultDBModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public long Count { get; set; } = 0;
        public List<AnswerDBModel> Answers { get; set; } = new List<AnswerDBModel>();
    }
}
