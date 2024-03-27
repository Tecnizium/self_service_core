namespace self_service_core.DTOs;

public class AdminDto
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    
    public AdminDto(string username, string password)
    {
        this.Username = username;
        this.Password = password;
    }
}