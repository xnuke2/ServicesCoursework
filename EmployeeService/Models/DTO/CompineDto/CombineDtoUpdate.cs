namespace EmployeeService.Models.DTO.CompineDto;

public class CombineDtoUpdate
{
    public int Id { get; set; }
    public string? Login { get; set; }
    public string? Password { get; set; }
    public string? Surname { get; set; }

    public string? Name { get; set; }
 
    public string? Patronymic { get; set; }

    public DateTime? BirthDate { get; set; }

    public int? RoleId { get; set; }

    public int? DivisionId { get; set; }
}