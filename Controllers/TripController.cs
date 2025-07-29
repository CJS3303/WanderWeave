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