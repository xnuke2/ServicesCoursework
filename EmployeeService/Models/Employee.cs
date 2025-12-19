namespace Bixd.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("employee")]
public class Employee
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [Column("surname")]
    [StringLength(20)]
    public string Surname { get; set; }
    
    [Required]
    [Column("name")]
    [StringLength(20)]
    public string Name { get; set; }
    
    [StringLength(20)]
    [Column("patronymic")]
    public string? Patronymic { get; set; }
    
    [Required]
    
    [Column("birth_date",TypeName = "date")]
    public DateTime BirthDate { get; set; }
    
    [Required]
    [Column("role_id")]
    public int RoleId { get; set; }
    
    [Required]
    [Column("divison_id")]
    public int DivisionId { get; set; }
    
    [NotMapped]
    public int Age => DateTime.Now.Year - BirthDate.Year - 
                      (DateTime.Now.DayOfYear < BirthDate.DayOfYear ? 1 : 0);
    // Навигационные свойства
    [ForeignKey("RoleId")]
    public virtual Role Role { get; set; }
    
}