using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project1.Areas.Identity.Data;
using Project1.Models;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Project1.Controllers;

[Authorize]
public class TripController : Controller
{
    private readonly Project1IdentityDbContext _dbContext;
    private readonly UserManager<IdentityUser> _userManager;

    public TripController(Project1IdentityDbContext dbContext, UserManager<IdentityUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var trips = await _dbContext.Trips
            .Where(x => x.UserId == user.Id)
            .ToListAsync();
        ViewBag.trips = trips;
        return View();
    }

    public IActionResult Add()
    {
        // Add country list for dropdown
        var countries = new List<string>
        {
            "United States", "Canada", "Mexico", "United Kingdom", "France",
            "Germany", "Italy", "Japan", "Australia", "Brazil"
        };
        ViewBag.Countries = new SelectList(countries);
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Add([FromForm] string name, [FromForm] string description, [FromForm] string location)
    {
        var user = await _userManager.GetUserAsync(User);
        
        var newTrip = new Trip()
        {
            Name = name,
            Description = description,
            Country = location,
            UserId = user.Id,
            CreationTime = DateTime.UtcNow
        };
        await _dbContext.AddAsync(newTrip);
        await _dbContext.SaveChangesAsync();
            
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete([FromForm] int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var trip = await _dbContext.Trips
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == user.Id);

        if (trip != null)
        {
            _dbContext.Remove(trip);
            await _dbContext.SaveChangesAsync();
        }
        
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Trip(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var trip = await _dbContext.Trips
            .Include(x => x.Days)
            .ThenInclude(x => x.Stops)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == user.Id);

        if (trip == null) return RedirectToAction("Index");

        ViewBag.trip = trip;
        return View();
    }

    public async Task<IActionResult> AddDay(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var trip = await _dbContext.Trips
            .Include(x => x.Days)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == user.Id);

        if (trip == null) return RedirectToAction("Index");

        var day = new Day()
        {
            DayNumber = trip.Days.Count,
            TripId = trip.Id
        };
        
        trip.Days.Add(day);
        await _dbContext.AddAsync(day);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction("Trip", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> AddStop([FromForm] string placeId, [FromForm] int dayId, 
        [FromForm] string name, [FromForm] string latlng)
    {
        var user = await _userManager.GetUserAsync(User);
        var day = await _dbContext.Days
            .Include(d => d.Stops)
            .FirstOrDefaultAsync(d => d.Id == dayId);

        if (day == null) return Content("unauthorized");

        var trip = await _dbContext.Trips
            .FirstOrDefaultAsync(x => x.Id == day.TripId && x.UserId == user.Id);
        
        if (trip == null) return Content("unauthorized");

        var stop = new Stop
        {
            placeid = placeId,
            Latlng = latlng,
            name = name,
            DayId = day.Id
        };
        
        day.Stops.Add(stop);
        await _dbContext.Stops.AddAsync(stop);
        await _dbContext.SaveChangesAsync();
        
        return Content("success");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteStop([FromForm] int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var stop = await _dbContext.Stops
            .Include(s => s.Day)
                .ThenInclude(d => d.Trip)
            .FirstOrDefaultAsync(s => s.Id == id && s.Day.Trip.UserId == user.Id);

        if (stop != null)
        {
            _dbContext.Stops.Remove(stop);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Trip", new { id = stop.Day.Trip.Id });
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteDay([FromForm] int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var day = await _dbContext.Days
            .Include(d => d.Trip)
            .Include(d => d.Stops)
            .FirstOrDefaultAsync(d => d.Id == id && d.Trip.UserId == user.Id);

        if (day != null)
        {
            _dbContext.Stops.RemoveRange(day.Stops);
            _dbContext.Days.Remove(day);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Trip", new { id = day.Trip.Id });
        }

        return RedirectToAction("Index");
    }
}