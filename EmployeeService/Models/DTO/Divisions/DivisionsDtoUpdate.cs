namespace Bixd.Models.DTO.Divisions;

public class DivisionsDtoUpdate
{
    public int Id { get; set; }
    public string? Name { get; set; }
    
    public int? ParentId { get; set; }
}