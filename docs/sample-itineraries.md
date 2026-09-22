# Sample itineraries

The homepage's **All Destinations** button opens `/destinations`. Each sample links
to its own public page, such as `/destinations/mauritius`. Reading samples does not
require an account and does not create or modify a user's trips.

Edit `Content/sample-itineraries.json` to change the published content. To add a
sample, append an object to the array with these fields:

- `slug`: a unique, lowercase URL name such as `japan-spring`.
- `destination`, `title`, `summary`: card and page text.
- `stay`: a short duration label, such as `8 days in Mauritius`.
- `travelDates`: the full range, including travel.
- `destinationDates`: the dates at the destination.
- `themes`: a list of short tags.
- `days`: dated entries, each containing an `activities` array with `time`,
  `activity`, and `notes` strings. Keep at least one activity per day.

Use an empty string for absent notes and `—` for an unspecified time. The order of
days and activities in the file is preserved. Text is encoded by Razor; HTML is
not supported. Unknown slugs return HTTP 404. Keep slugs unique without regard to
case. This is a file-based catalog, not a browser editing interface.

The content file is included in build/publish output. Deploy the updated file when
adding samples to a hosted app; local content edits appear on the next request.
The supplied Mauritius plan contains 24 activities across February 1–10, with an
8-day stay from February 2–9. Its dates, times, and notes are sample content supplied
by the site owner, not live flight or activity availability.
