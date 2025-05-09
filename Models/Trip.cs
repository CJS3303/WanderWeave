using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Project1.Models;

public class Trip
{   
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column(TypeName = "tinytext")]
    public string Name { get; set; }
    
    [Column(TypeName = "text")]
    public string Description { get; set; }
    
    public DateTime CreationTime { get; set; }

    // Add foreign key for IdentityUser
    public string UserId { get; set; }
    public IdentityUser User { get; set; }
    
    [Column(TypeName = "tinytext")]
    public string Country { get; set; }
    
    // Use consistent naming (Days instead of days)
    public ICollection<Day> Days { get; set; } = new List<Day>();
}