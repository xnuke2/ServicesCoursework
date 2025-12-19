namespace Bixd.Models.DTO.Employees;

public class EmployeesDto
{

    public string Surname { get; set; }

    public string Name { get; set; }
 
    public string? Patronymic { get; set; }

    public DateTime BirthDate { get; set; }

    public int RoleId { get; set; }

    public int DivisionId { get; set; }
}