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
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddDay(int id, [FromForm] string? name)
    {
        var user = await _userManager.GetUserAsync(User);
        var trip = await _dbContext.Trips
            .Include(x => x.Days)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == user.Id);

        if (trip == null) return RedirectToAction("Index");

        if (name?.Trim().Length > 100)
            return BadRequest("Day names must be at most 100 characters.");

        var orderedDays = trip.Days.OrderBy(d => d.DayNumber).ThenBy(d => d.Id).ToList();
        for (var i = 0; i < orderedDays.Count; i++)
            orderedDays[i].DayNumber = i;

        var day = new Day()
        {
            Name = string.IsNullOrWhiteSpace(name) ? $"Untitled {trip.Days.Count + 1}" : name.Trim(),
            DayNumber = trip.Days.Count,
            TripId = trip.Id
        };
        
        trip.Days.Add(day);
        await _dbContext.AddAsync(day);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction("Trip", new { id });
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RenameDay(int id, [FromForm] string? name)
    {
        if (name?.Trim().Length > 100)
            return BadRequest("Day names must be at most 100 characters.");
        var userId = _userManager.GetUserId(User);
        var day = await _dbContext.Days
            .FirstOrDefaultAsync(d => d.Id == id && d.Trip.UserId == userId);
        if (day == null) return NotFound();
        day.Name = string.IsNullOrWhiteSpace(name) ? $"Untitled {day.DayNumber + 1}" : name.Trim();
        await _dbContext.SaveChangesAsync();
        return RedirectToAction("Trip", new { id = day.TripId });
    }

    [HttpGet]
    public async Task<IActionResult> Index(string sortOrder = "default")
    {
        var user = await _userManager.GetUserAsync(User);

        // base query
        var query = _dbContext.Trips
            .Where(x => x.UserId == user.Id);

        // optional sorting
        query = sortOrder switch
        {
            "recent"       => query.OrderByDescending(t => t.CreationTime),
            "alphabetical" => query.OrderBy(t => t.Name),
            _              => query.OrderBy(t => t.Id)
        };

        var trips = await query.ToListAsync();

        ViewBag.trips      = trips;
        ViewBag.SortOrder  = sortOrder;
        ViewBag.TotalTrips = trips.Count;
        ViewBag.Countries = new List<string>
        {
            "Afghanistan",
  "Albania",
  "Algeria",
  "Andorra",
  "Angola",
  "Antigua and Barbuda",
  "Argentina",
  "Armenia",
  "Australia",
  "Austria",
  "Azerbaijan",
  "Bahamas",
  "Bahrain",
  "Bangladesh",
  "Barbados",
  "Belarus",
  "Belgium",
  "Belize",
  "Benin",
  "Bhutan",
  "Bolivia",
  "Bosnia and Herzegovina",
  "Botswana",
  "Brazil",
  "Brunei",
  "Bulgaria",
  "Burkina Faso",
  "Burundi",
  "Cabo Verde",
  "Cambodia",
  "Cameroon",
  "Canada",
  "Central African Republic",
  "Chad",
  "Chile",
  "China",
  "Colombia",
  "Comoros",
  "Congo, Republic of the",
  "Congo, Democratic Republic of the",
  "Costa Rica",
  "Côte d'Ivoire",
  "Croatia",
  "Cuba",
  "Cyprus",
  "Czech Republic",
  "Denmark",
  "Djibouti",
  "Dominica",
  "Dominican Republic",
  "Ecuador",
  "Egypt",
  "El Salvador",
  "Equatorial Guinea",
  "Eritrea",
  "Estonia",
  "Eswatini",
  "Ethiopia",
  "Fiji",
  "Finland",
  "France",
  "Gabon",
  "Gambia",
  "Georgia",
  "Germany",
  "Ghana",
  "Greece",
  "Grenada",
  "Guatemala",
  "Guinea",
  "Guinea-Bissau",
  "Guyana",
  "Haiti",
  "Honduras",
  "Hungary",
  "Iceland",
  "India",
  "Indonesia",
  "Iran",
  "Iraq",
  "Ireland",
  "Israel",
  "Italy",
  "Jamaica",
  "Japan",
  "Jordan",
  "Kazakhstan",
  "Kenya",
  "Kiribati",
  "Korea, North",
  "Korea, South",
  "Kosovo",
  "Kuwait",
  "Kyrgyzstan",
  "Laos",
  "Latvia",
  "Lebanon",
  "Lesotho",
  "Liberia",
  "Libya",
  "Liechtenstein",
  "Lithuania",
  "Luxembourg",
  "Madagascar",
  "Malawi",
  "Malaysia",
  "Maldives",
  "Mali",
  "Malta",
  "Marshall Islands",
  "Mauritania",
  "Mauritius",
  "Mexico",
  "Micronesia",
  "Moldova",
  "Monaco",
  "Mongolia",
  "Montenegro",
  "Morocco",
  "Mozambique",
  "Myanmar",
  "Namibia",
  "Nauru",
  "Nepal",
  "Netherlands",
  "New Zealand",
  "Nicaragua",
  "Niger",
  "Nigeria",
  "North Macedonia",
  "Norway",
  "Oman",
  "Pakistan",
  "Palau",
  "Panama",
  "Papua New Guinea",
  "Paraguay",
  "Peru",
  "Philippines",
  "Poland",
  "Portugal",
  "Qatar",
  "Romania",
  "Russia",
  "Rwanda",
  "Saint Kitts and Nevis",
  "Saint Lucia",
  "Saint Vincent and the Grenadines",
  "Samoa",
  "San Marino",
  "Sao Tome and Principe",
  "Saudi Arabia",
  "Senegal",
  "Serbia",
  "Seychelles",
  "Sierra Leone",
  "Singapore",
  "Slovakia",
  "Slovenia",
  "Solomon Islands",
  "Somalia",
  "South Africa",
  "Spain",
  "Sri Lanka",
  "Sudan",
  "Suriname",
  "Sweden",
  "Switzerland",
  "Syria",
  "Taiwan",
  "Tajikistan",
  "Tanzania",
  "Thailand",
  "Timor-Leste",
  "Togo",
  "Tonga",
  "Trinidad and Tobago",
  "Tunisia",
  "Turkey",
  "Turkmenistan",
  "Tuvalu",
  "Uganda",
  "Ukraine",
  "United Arab Emirates",
  "United Kingdom",
  "United States",
  "Uruguay",
  "Uzbekistan",
  "Vanuatu",
  "Vatican City",
  "Venezuela",
  "Vietnam",
  "Yemen",
  "Zambia",
  "Zimbabwe"
        };
        return View();
    }

    public IActionResult Add()
    {
        // Add country list for dropdown
        var countries = new List<string>
        {
            "Afghanistan",
  "Albania",
  "Algeria",
  "Andorra",
  "Angola",
  "Antigua and Barbuda",
  "Argentina",
  "Armenia",
  "Australia",
  "Austria",
  "Azerbaijan",
  "Bahamas",
  "Bahrain",
  "Bangladesh",
  "Barbados",
  "Belarus",
  "Belgium",
  "Belize",
  "Benin",
  "Bhutan",
  "Bolivia",
  "Bosnia and Herzegovina",
  "Botswana",
  "Brazil",
  "Brunei",
  "Bulgaria",
  "Burkina Faso",
  "Burundi",
  "Cabo Verde",
  "Cambodia",
  "Cameroon",
  "Canada",
  "Central African Republic",
  "Chad",
  "Chile",
  "China",
  "Colombia",
  "Comoros",
  "Congo, Republic of the",
  "Congo, Democratic Republic of the",
  "Costa Rica",
  "Côte d'Ivoire",
  "Croatia",
  "Cuba",
  "Cyprus",
  "Czech Republic",
  "Denmark",
  "Djibouti",
  "Dominica",
  "Dominican Republic",
  "Ecuador",
  "Egypt",
  "El Salvador",
  "Equatorial Guinea",
  "Eritrea",
  "Estonia",
  "Eswatini",
  "Ethiopia",
  "Fiji",
  "Finland",
  "France",
  "Gabon",
  "Gambia",
  "Georgia",
  "Germany",
  "Ghana",
  "Greece",
  "Grenada",
  "Guatemala",
  "Guinea",
  "Guinea-Bissau",
  "Guyana",
  "Haiti",
  "Honduras",
  "Hungary",
  "Iceland",
  "India",
  "Indonesia",
  "Iran",
  "Iraq",
  "Ireland",
  "Israel",
  "Italy",
  "Jamaica",
  "Japan",
  "Jordan",
  "Kazakhstan",
  "Kenya",
  "Kiribati",
  "Korea, North",
  "Korea, South",
  "Kosovo",
  "Kuwait",
  "Kyrgyzstan",
  "Laos",
  "Latvia",
  "Lebanon",
  "Lesotho",
  "Liberia",
  "Libya",
  "Liechtenstein",
  "Lithuania",
  "Luxembourg",
  "Madagascar",
  "Malawi",
  "Malaysia",
  "Maldives",
  "Mali",
  "Malta",
  "Marshall Islands",
  "Mauritania",
  "Mauritius",
  "Mexico",
  "Micronesia",
  "Moldova",
  "Monaco",
  "Mongolia",
  "Montenegro",
  "Morocco",
  "Mozambique",
  "Myanmar",
  "Namibia",
  "Nauru",
  "Nepal",
  "Netherlands",
  "New Zealand",
  "Nicaragua",
  "Niger",
  "Nigeria",
  "North Macedonia",
  "Norway",
  "Oman",
  "Pakistan",
  "Palau",
  "Panama",
  "Papua New Guinea",
  "Paraguay",
  "Peru",
  "Philippines",
  "Poland",
  "Portugal",
  "Qatar",
  "Romania",
  "Russia",
  "Rwanda",
  "Saint Kitts and Nevis",
  "Saint Lucia",
  "Saint Vincent and the Grenadines",
  "Samoa",
  "San Marino",
  "Sao Tome and Principe",
  "Saudi Arabia",
  "Senegal",
  "Serbia",
  "Seychelles",
  "Sierra Leone",
  "Singapore",
  "Slovakia",
  "Slovenia",
  "Solomon Islands",
  "Somalia",
  "South Africa",
  "Spain",
  "Sri Lanka",
  "Sudan",
  "Suriname",
  "Sweden",
  "Switzerland",
  "Syria",
  "Taiwan",
  "Tajikistan",
  "Tanzania",
  "Thailand",
  "Timor-Leste",
  "Togo",
  "Tonga",
  "Trinidad and Tobago",
  "Tunisia",
  "Turkey",
  "Turkmenistan",
  "Tuvalu",
  "Uganda",
  "Ukraine",
  "United Arab Emirates",
  "United Kingdom",
  "United States",
  "Uruguay",
  "Uzbekistan",
  "Vanuatu",
  "Vatican City",
  "Venezuela",
  "Vietnam",
  "Yemen",
  "Zambia",
  "Zimbabwe"
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

    

    [HttpPost]
    public async Task<IActionResult> AddStop([FromForm] string placeId, [FromForm] int dayId, 
        [FromForm] string name, [FromForm] string latlng,
        [FromForm] TimeSpan? arrivalTime, [FromForm] TimeSpan? departureTime, [FromForm] string? notes)
    {
        var user = await _userManager.GetUserAsync(User);
        var day = await _dbContext.Days
            .Include(d => d.Stops)
            .FirstOrDefaultAsync(d => d.Id == dayId);

        if (day == null) return NotFound("Day not found.");

        var trip = await _dbContext.Trips
            .FirstOrDefaultAsync(x => x.Id == day.TripId && x.UserId == user.Id);
        
        if (trip == null) return NotFound("Day not found.");

        if (string.IsNullOrWhiteSpace(placeId) || string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(latlng))
            return BadRequest("Select a location before adding a stop.");

        if (!ModelState.IsValid || !ValidStopTime(arrivalTime) || !ValidStopTime(departureTime))
            return BadRequest("Enter valid arrival and departure times.");
        if (notes?.Length > 4000) return BadRequest("Notes must be at most 4,000 characters.");

        var stop = new Stop
        {
            ArrivalTime = arrivalTime,
            DepartureTime = departureTime,
            Notes = notes?.Trim() ?? "",
            placeid = placeId,
            Latlng = latlng,
            name = name,
            SortOrder = day.Stops.Count == 0 ? 0 : day.Stops.Max(s => s.SortOrder) + 1,
            DayId = day.Id
        };
        
        day.Stops.Add(stop);
        await _dbContext.Stops.AddAsync(stop);
        await _dbContext.SaveChangesAsync();
        
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveStopNotes(int id, [FromForm] string? notes,
        [FromForm] TimeSpan? arrivalTime, [FromForm] TimeSpan? departureTime)
    {
        if (notes?.Length > 4000) return BadRequest("Notes must be at most 4,000 characters.");
        var userId = _userManager.GetUserId(User);
        var stop = await _dbContext.Stops.Include(s => s.Day)
            .FirstOrDefaultAsync(s => s.Id == id && s.Day.Trip.UserId == userId);
        if (stop == null) return NotFound();
        if (!ModelState.IsValid || !ValidStopTime(arrivalTime) || !ValidStopTime(departureTime))
            return BadRequest("Enter valid arrival and departure times.");
        stop.ArrivalTime = arrivalTime;
        stop.DepartureTime = departureTime;
        stop.Notes = notes?.Trim() ?? "";
        await _dbContext.SaveChangesAsync();
        return RedirectToAction("Trip", new { id = stop.Day.TripId });
    }

    private static bool ValidStopTime(TimeSpan? time) =>
        !time.HasValue || (time.Value >= TimeSpan.Zero && time.Value < TimeSpan.FromDays(1));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveStop([FromForm] int id, [FromForm] int dayId,
        [FromForm] int position)
    {
        var userId = _userManager.GetUserId(User);
        var stop = await _dbContext.Stops.Include(s => s.Day)
            .FirstOrDefaultAsync(s => s.Id == id && s.Day.Trip.UserId == userId);
        if (stop == null) return NotFound();
        var target = await _dbContext.Days.FirstOrDefaultAsync(d => d.Id == dayId &&
            d.TripId == stop.Day.TripId && d.Trip.UserId == userId);
        if (target == null) return NotFound();

        var sourceId = stop.DayId;
        var affected = await _dbContext.Stops
            .Where(s => s.DayId == sourceId || s.DayId == dayId)
            .OrderBy(s => s.SortOrder).ThenBy(s => s.Id).ToListAsync();
        var destination = affected.Where(s => s.DayId == dayId && s.Id != id).ToList();
        if (position < 0 || position > destination.Count) return BadRequest();
        destination.Insert(position, stop);
        stop.Day = target;
        stop.DayId = target.Id;
        for (var i = 0; i < destination.Count; i++) destination[i].SortOrder = i;
        if (sourceId != dayId)
        {
            var source = affected.Where(s => s.DayId == sourceId && s.Id != id).ToList();
            for (var i = 0; i < source.Count; i++) source[i].SortOrder = i;
        }
        await _dbContext.SaveChangesAsync();
        return Ok();
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

        if (day == null) return NotFound("Day not found.");

        var remainingDays = await _dbContext.Days
            .Where(d => d.TripId == day.TripId && d.Id != day.Id)
            .OrderBy(d => d.DayNumber).ThenBy(d => d.Id)
            .ToListAsync();
        for (var i = 0; i < remainingDays.Count; i++)
            remainingDays[i].DayNumber = i;

        _dbContext.Stops.RemoveRange(day.Stops);
        _dbContext.Days.Remove(day);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
}
