using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project1.Models;

namespace Project1.Controllers;

[AllowAnonymous]
[Route("destinations")]
public sealed class DestinationsController : Controller
{
    private readonly IWebHostEnvironment _environment;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public DestinationsController(IWebHostEnvironment environment) => _environment = environment;

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
        => View(await ReadSamples(cancellationToken));

    [HttpGet("{slug}")]
    public async Task<IActionResult> Itinerary(string slug, CancellationToken cancellationToken)
    {
        var samples = await ReadSamples(cancellationToken);
        var sample = samples.SingleOrDefault(item => string.Equals(item.Slug, slug, StringComparison.OrdinalIgnoreCase));
        return sample is null ? NotFound() : View(sample);
    }

    private async Task<SampleItinerary[]> ReadSamples(CancellationToken cancellationToken)
    {
        // The URL only selects a catalog entry; it is never used as a filesystem path.
        var path = Path.Combine(_environment.ContentRootPath, "Content", "sample-itineraries.json");
        await using var stream = System.IO.File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<SampleItinerary[]>(stream, JsonOptions, cancellationToken) ?? [];
    }
}
