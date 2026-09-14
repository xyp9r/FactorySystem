namespace FactorySystem.Models;

public class CreateDetailDto
{
    public string Name { get; set; }
    
    public int Count { get; set; }
    
    public string Status { get; set; }
    
    public int CreatorId { get; set; }
}