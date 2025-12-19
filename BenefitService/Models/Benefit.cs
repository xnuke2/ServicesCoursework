using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bixd.Models;

[Table("benefits")]
public class Benefit
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; }
    
    [Required]
    [Column("add_day")]
    public int AddDay { get; set; }
    

}