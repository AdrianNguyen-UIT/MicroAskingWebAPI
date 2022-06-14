using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDBDataAccess.Models
{
    public class AnswerDBModel
    {
        public float Score_Start { get; set; } = 0.0f;
        public float Score_End { get; set; } = 0.0f;
        public string Answer { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public float Score { get; set; } = 0.0f;
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
