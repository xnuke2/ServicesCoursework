using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeService.Models;

[Table("authdata")]
public class AuthData
{
    [Key]
    [Column("employeeid")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    [Column("login")]
    [StringLength(20)]
    public string Login { get; set; }
    [Required]
    [Column("password")]
    [StringLength(50)]
    public string Password { get; set; }
}