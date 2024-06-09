using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using self_service_core.DTOs;
using ThirdParty.Json.LitJson;

namespace self_service_core.Models;

public class WaiterModel
{
    [JsonPropertyName("id")]
    [BsonId]
    public string WaiterId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public string Cpf { get; set; }
    public DateTime StartOfWorkShift { get; set; }
    public DateTime EndOfWordShift { get; set; }
    public double Commission { get; set; }
    public WaiterStatus Status { get; set; }
    public string ProfilePicture { get; set; }

    public WaiterModel(CreateWaiterDto waiter)
    {
        Name = waiter.Name;
        Cpf = waiter.Cpf;
        StartOfWorkShift = waiter.StartOfWorkShift;
        EndOfWordShift = waiter.EndOfWordShift;
        Commission = waiter.Commission;
        Status = waiter.Status;
        ProfilePicture = waiter.ProfilePicture;
    }
    
    public WaiterModel(UpdateWaiterDto waiter)
    {
        Name = waiter.Name;
        Cpf = waiter.Cpf;
        StartOfWorkShift = waiter.StartOfWorkShift;
        EndOfWordShift = waiter.EndOfWordShift;
        Commission = waiter.Commission;
        Status = waiter.Status;
        ProfilePicture = waiter.ProfilePicture;
    }
}
public enum WaiterStatus
{
    Available,
    OnDuty,
    OnVacation,
    Off
}