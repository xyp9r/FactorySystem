namespace FactorySystem.Models;

public class DetailResponseDto : BaseEntity
{
    public int Count { get; set; }
    
    public string Status { get; set; }
    
    public int CreatorId { get; set; }
}