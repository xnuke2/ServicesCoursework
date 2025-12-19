namespace BenefitService.Models.DTO.EmployeeBenefits;

public class EmployeeBenefitDto
{
    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int EmployeeId { get; set; }

    public int BenefitsId { get; set; }
}