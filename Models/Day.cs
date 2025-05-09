using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project1.Models;

public class Day
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int DayNumber { get; set; }  // Renamed from 'day' for clarity
    
    // Add relationship to Trip
    public int TripId { get; set; }
    public Trip Trip { get; set; }
    
    // Use consistent naming (Stops instead of stops)
    public ICollection<Stop> Stops { get; set; } = new List<Stop>();
}