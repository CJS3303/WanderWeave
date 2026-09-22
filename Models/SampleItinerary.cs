namespace Project1.Models;

public sealed class SampleItinerary
{
    public required string Slug { get; init; }
    public required string Destination { get; init; }
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string Stay { get; init; }
    public required string TravelDates { get; init; }
    public required string DestinationDates { get; init; }
    public required string[] Themes { get; init; }
    public required SampleItineraryDay[] Days { get; init; }
}

public sealed class SampleItineraryDay
{
    public required string Date { get; init; }
    public required SampleItineraryActivity[] Activities { get; init; }
}

public sealed class SampleItineraryActivity
{
    public required string Time { get; init; }
    public required string Activity { get; init; }
    public string Notes { get; init; } = "";
}
