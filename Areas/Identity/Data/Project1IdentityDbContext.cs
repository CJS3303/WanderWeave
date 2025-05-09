using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project1.Models;

namespace Project1.Areas.Identity.Data;

public class Project1IdentityDbContext : IdentityDbContext<IdentityUser>
{
    public Project1IdentityDbContext(DbContextOptions<Project1IdentityDbContext> options)
        : base(options)
    {
    }
    public DbSet<Trip> Trips { get; set; }
    
    public DbSet<Day> Days { get; set; }
    
    public DbSet<Stop> Stops { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
