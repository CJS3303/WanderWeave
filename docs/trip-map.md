# Itinerary map

The itinerary displays saved stops beside a Google map. Stops use their persisted
Latlng coordinates (latitude_longitude); no additional geocoding is requested.
Invalid coordinates are skipped with a visible notice. Clicking a stop focuses its
pin; clicking a pin shows its name and scrolls to the stop. Pin labels use day.stop
numbers and follow drag-and-drop changes. New stops appear after successful saving
and the existing page reload. There is no map preview in the Add Stop dialog.

The map uses the existing Google Maps JavaScript API key and Advanced Markers.
For local testing the map ID defaults to Google's DEMO_MAP_ID. Before production,
create a JavaScript map ID in Google Cloud and set GoogleMaps:MapId in configuration
(or GoogleMaps__MapId as a Heroku config variable). This is a map ID, not the API key.

Build verification passed. Live map rendering and responsive layout have not been
browser-tested in this session.

## Route comparison and notes

Select two stop checkboxes in order: first is origin A, second is destination B.
Each day also has a Preview day button. Preview shows only that day's pins using
the existing colour and day.stop labels. A route panel inside the map lists all
stops in order with clickable numbered entries and the total driving distance.
Google driving polylines connect each consecutive pair. The map automatically fits
all pins and route paths, leaving room for the panel. On narrow screens the panel
sits above the map. Clicking the active Preview day button again restores all pins
and removes the day route lines. Routine map instructions and the header are hidden;
loading and error messages remain visible when needed.
Selecting a route checkbox exits preview and starts a new A/B comparison.

The day summary adds Google's driving distance for every consecutive pair of
stops in itinerary order, without a return leg to the first stop. It uses
Route.computeRoutes with DRIVING, TRAFFIC_UNAWARE, distanceMeters and path, independent
of the A/B departure-time picker. See the [Google Routes reference](https://developers.google.com/maps/documentation/javascript/reference/route).
Requests run at most two at a time per calculation; successful pair results are
cached in memory for five minutes. An unavailable section makes the total
unavailable (with retry) and no complete route is drawn, rather than displaying a partial sum. Empty/single-stop
days have zero travel distance and make no route requests. Settled drag moves,
including rollbacks, recalculate the preview, and stale responses are ignored.

The right-hand map compares Car, Bus, and Walk with three independent requests.
Only the active mode is drawn. Mode switching reuses loaded responses without
another request; swapping endpoints, retrying, or applying departure changes loads
fresh estimates. No intermediate itinerary stops are sent. Car uses TRAFFIC_AWARE;
Bus uses TRANSIT with allowedTransitModes BUS and includes walking connections.
The panel displays Google-provided boarding stops, lines, transfers, walking segments,
and warnings where available. Errors and unavailable routes are per-mode.
Consecutive walking instructions are combined into one connection, with duration
rounded after summing. Bus entries show departure and arrival times when available;
the overall journey estimate remains the duration returned by Google.

Depart defaults to Now. Scheduled departure uses the browser time zone, displayed
beside the date/time field; car and bus receive that instant. Walking is independent
of departure. Past/empty dates are rejected, and bus schedules beyond Google's
100-day window show no route. Stop arrival/departure fields remain separate.
Route details and the comparison panel stay on the map; on narrow screens the panel
sits directly above the map so it does not obscure the route.
The implementation uses the Maps JavaScript routes library and Route.computeRoutes.
Enable Routes API on the key's Google Cloud project and permit it in API restrictions.
Keep the existing Maps JavaScript API and Places API (New) enabled.

Notes are optional text (maximum 4,000 characters) saved through an ownership-checked,
antiforgery-protected action. The AddStopNotes migration was applied directly to the
configured database because of its previously documented migration-history issue.
Other deployments require that schema change before this code is deployed.

Run `node tests/route-selection.mjs` for mocked tests of all three modes, BUS filtering,
independent loading and errors, transit details, cached mode switching, endpoint order,
swapping, clearing, stale responses, schedule validation, and mode-specific requests. These checks passed; live Google routing and browser layout
remain unverified. No Google Cloud settings were changed by this implementation.

## Stop times and editor

ArrivalTime and DepartureTime are optional local times stored as MySQL time(6).
The AddStopTimes migration was applied to the configured database. Existing stops
keep unset times. Add Stop and Times & notes both save arrival/departure and notes.
A departure earlier than arrival represents the following day, shown as +1 day.
These planned times do not automatically change the route panel's chosen departure.

The editor shows the selected location only in the search field, responsive time fields, and a
collapsed timing summary. Build and mocked editor submission/reset checks passed;
visual browser verification remains outstanding.
