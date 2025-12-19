

namespace Bixd.Models;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("divison")]
public class Division
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; }
    
    // Внешний ключ для иерархической структуры
    [Column("parent_id")]
    public int? ParentId { get; set; }
    
    // Навигационные свойства
    [ForeignKey("ParentId")]
    public virtual Division Parent { get; set; }
    
    public virtual ICollection<Division> Children { get; set; }
    public virtual ICollection<Employee> Employees { get; set; }
}