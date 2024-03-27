using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace self_service_core.Models;

public class PrinterModel
{
    [BsonId]
    [JsonIgnore]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Host { get; set; }
    public string Port { get; set; }
    public List<string>? CategoryIds { get; set; }
    
    public bool? isDefault { get; set; }
    
    public PrinterModel(string host, string port, List<string>? categoryIds, bool? isDefault)
    {
        this.Host = host;
        this.Port = port;
        this.CategoryIds = categoryIds;
        this.isDefault = isDefault ?? false;
    }
}