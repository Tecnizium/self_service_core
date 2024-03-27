using MongoDB.Bson.Serialization.Attributes;
using self_service_core.DTOs;

namespace self_service_core.Models;

public class AdminModel
{
    [BsonId]
    public string? AdminId { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    
    public AdminModel(string username, string password)
    {
        this.AdminId = Guid.NewGuid().ToString();
        this.Username = username;
        this.Password = password;
    }
    
    public AdminModel(AdminDto admin)
    {
        this.AdminId = Guid.NewGuid().ToString();
        this.Username = admin.Username;
        this.Password = admin.Password;
    }
}