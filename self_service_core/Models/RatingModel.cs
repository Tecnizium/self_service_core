using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace self_service_core.Models;

public class RatingModel
{
    [JsonIgnore]
    [BsonId]
    public string RatingId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public IEnumerable<string> Types { get; set; }
    
}