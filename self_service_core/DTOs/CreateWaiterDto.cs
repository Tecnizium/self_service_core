using self_service_core.Models;

namespace self_service_core.DTOs;

public class CreateWaiterDto
{
    public string Name { get; set; }
    public string Cpf { get; set; }
    public DateTime StartOfWorkShift { get; set; }
    public DateTime EndOfWordShift { get; set; }
    public double Commission { get; set; }
    public WaiterStatus Status { get; set; }
    public string ProfilePicture { get; set; }
}