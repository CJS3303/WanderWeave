using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project1.Models;

public class Stop
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    // Correct navigation property name (Day instead of day)
    public Day Day { get; set; }
    
    // Add foreign key
    public int DayId { get; set; }
    
    [Column(TypeName = "tinytext")]
    public string name { get; set; }
    
    [Column(TypeName = "tinytext")]
    public string Latlng { get; set; }
    
    [Column(TypeName = "tinytext")]
    public string placeid { get; set; }
}