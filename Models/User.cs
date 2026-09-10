namespace FactorySystem.Models;

public class User : BaseEntity
{
    public string PasswordHash { get; set; }
    public string Role { get; set; }
}