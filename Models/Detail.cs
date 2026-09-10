namespace FactorySystem.Models;

public class Detail
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public int Count { get; set; }
    
    public string Status { get; set; }
    
    public int CreatorId { get; set; }
    
    public User Creator { get; set; }
}