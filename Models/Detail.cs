namespace FactorySystem.Models;

public class Detail : BaseEntity
{
    public int Count { get; set; }
    
    public string Status { get; set; }
    
    public int CreatorId { get; set; }
    
    public User Creator { get; set; }
}