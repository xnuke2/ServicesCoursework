namespace Bixd.Models;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("employee_benefits")]
public class EmployeeBenefit
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    
    [Column("start_date",TypeName = "date")]
    public DateTime StartDate { get; set; }
    
    [Required]
    [Column("end_date",TypeName = "date")]
    public DateTime EndDate { get; set; }
    
    [Required]
    [Column("employee_id")]
    public int EmployeeId { get; set; }
    
    [Required]
    [Column("benefits_id")]
    public int BenefitsId { get; set; }
    
    
    [ForeignKey("BenefitsId")]
    public virtual Benefit Benefit { get; set; }
}