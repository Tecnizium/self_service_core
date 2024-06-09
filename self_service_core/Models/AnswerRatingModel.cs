using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace self_service_core.Models;

public class AnswerRatingModel
{
    [BsonId]
    [JsonIgnore]
    public string AnswerRatingId { get; set; } = Guid.NewGuid().ToString();
    public string RatingId { get; set; }
    public string OrderId { get; set; }
    public string Comment { get; set; }
    public double Score { get; set; }
    public IEnumerable<int> Scores { get; set; }
    public DateTime CreatedAt { get; set; }
}