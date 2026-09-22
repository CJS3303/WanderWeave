(() => {
    const panel = document.getElementById('trip-map');
    const status = document.getElementById('map-status');
    const itinerary = document.getElementById('trip-days');
    let map, Marker, infoWindow, markers = [];
    let failed = false;
    let selectedStops = [], routeLines = [], routeVersion = 0;
    let activeMode = 'DRIVING', results = {}, drawnRoute;
    let previewDayId = null, dayDistanceVersion = 0;
    let dayRoutes = [], dayRouteLines = [];
    const drivingDistanceCache = new Map();
    const modeNames = { DRIVING: 'Car', TRANSIT: 'Bus', WALKING: 'Walk' };
    const modes = Object.keys(modeNames);
    const el = id => document.getElementById(id);
    const routeSummary = el('route-summary');
    const clearRouteButton = el('clear-route');
    const departureToggle = el('route-departure-toggle');
    let confirmedDeparture = null;
    const departureInput = el('route-departure-time');
    const timezone = Intl.DateTimeFormat().resolvedOptions().timeZone;
    el('route-timezone').textContent = `Date & time (${timezone})`;

    function duration(milliseconds) {
        const minutes = Math.max(1, Math.ceil(milliseconds / 60000));
        return minutes >= 60 ? `${Math.floor(minutes / 60)} hr ${minutes % 60} min` : `${minutes} min`;
    }
    function distance(meters) {
        return meters < 1000 ? `${Math.round(meters)} m` : `${(meters / 1000).toFixed(1)} km`;
    }
    function setMapStatus(message) {
        status.textContent = message;
        status.hidden = !message;
    }
    function previewList() {
        return [...itinerary.querySelectorAll('.stop-list')].find(list => list.dataset.dayId === previewDayId);
    }
    function removeDayRoute() {
        dayRouteLines.forEach(line => line.setMap(null));
        dayRouteLines = [];
        dayRoutes = [];
    }
    function showDayStops(list, rows) {
        const stops = el('day-route-stops');
        stops.replaceChildren();
        const dayNumber = [...itinerary.querySelectorAll('.stop-list')].indexOf(list) + 1;
        rows.forEach((row, index) => {
            const item = document.createElement('li');
            const button = document.createElement('button');
            button.type = 'button';
            button.setAttribute('aria-label', `Show stop ${index + 1}: ${row.dataset.stopName} on map`);
            const number = document.createElement('span');
            number.className = 'day-route-number';
            number.textContent = `${dayNumber}.${index + 1}`;
            const name = document.createElement('span');
            name.className = 'day-route-name';
            name.textContent = row.dataset.stopName;
            button.appendChild(number);
            button.appendChild(name);
            button.addEventListener('click', () => {
                const entry = markers.find(entry => entry.row === row);
                if (entry) select(entry, false);
                else setMapStatus('This stop has no valid map coordinates.');
            });
            item.appendChild(button);
            stops.appendChild(item);
        });
    }
    function previewDay(dayId) {
        clearRoute();
        removeDayRoute();
        previewDayId = dayId;
        ++dayDistanceVersion;
        el('day-preview').hidden = dayId === null;
        el('route-overlay').hidden = dayId !== null;
        itinerary.querySelectorAll('.preview-day').forEach(button => {
            button.setAttribute('aria-pressed', String(button.dataset.dayId === dayId));
        });
        render();
        fit();
        if (dayId !== null) {
            loadDayDistance();
            if (window.matchMedia('(max-width: 900px)').matches)
                el('day-preview').scrollIntoView({behavior:'smooth', block:'start'});
        }
    }
    async function loadDayDistance() {
        const version = ++dayDistanceVersion;
        removeDayRoute();
        const list = previewList();
        if (!list) return;
        const rows = [...list.querySelectorAll('[data-stop-id]')];
        showDayStops(list, rows);
        const summary = el('day-preview-distance');
        el('day-preview-name').textContent = list.dataset.dayName;
        el('retry-day-distance').hidden = true;
        if (rows.length < 2) {
            summary.textContent = rows.length ? '1 stop · 0 km driving' : 'No stops yet · 0 km driving';
            return;
        }
        if (!map || failed) {
            summary.textContent = 'Driving distance unavailable while the map is not connected.';
            return;
        }
        summary.textContent = `${rows.length} stops · Calculating driving distance…`;
        // Snapshot the order so dragging cannot change a request already in progress.
        const stops = rows.map(row => ({placeId:row.dataset.placeId, latlng:row.dataset.latlng}));
        const routes = new Array(stops.length - 1);
        let next = 0;
        try {
            const [{ Route }, { Place }] = await withTimeout(() => Promise.all([
                google.maps.importLibrary('routes'), google.maps.importLibrary('places')
            ]));
            const endpoint = stop => stop.placeId ? new Place({id:stop.placeId}) : coordinates(stop.latlng);
            const worker = async () => {
                while (version === dayDistanceVersion && next < routes.length) {
                    const index = next++;
                    const pair = stops.slice(index, index + 2);
                    const key = JSON.stringify(pair);
                    let cached = drivingDistanceCache.get(key);
                    if (!cached || Date.now() - cached.created > 5 * 60000) {
                        const work = withTimeout(async () => {
                            const origin = endpoint(pair[0]), destination = endpoint(pair[1]);
                            if (!origin || !destination) throw new Error('Missing stop location');
                            const response = await Route.computeRoutes({
                                origin, destination, travelMode:'DRIVING', routingPreference:'TRAFFIC_UNAWARE',
                                fields:['distanceMeters','path'], language:'en'
                            });
                            const route = response.routes?.[0];
                            if (!Number.isFinite(route?.distanceMeters) || route.distanceMeters < 0 || !route.path?.length)
                                throw new Error('No driving route available');
                            return route;
                        });
                        cached = {work, created:Date.now()};
                        drivingDistanceCache.set(key, cached);
                        work.catch(() => {
                            if (drivingDistanceCache.get(key)?.work === work) drivingDistanceCache.delete(key);
                        });
                    }
                    try { routes[index] = await cached.work; }
                    catch { routes[index] = null; }
                }
            };
            // Bound API concurrency even for a day with many stops.
            await Promise.all([worker(), worker()]);
            if (version !== dayDistanceVersion) return;
            const missing = routes.map((value, index) => value === null ? `${index + 1} → ${index + 2}` : null).filter(Boolean);
            if (missing.length) {
                summary.textContent = `Total driving distance unavailable. Could not get a driving route for ${missing.length === 1 ? 'section' : 'sections'} ${missing.join(', ')}.`;
                el('retry-day-distance').hidden = false;
                return;
            }
            dayRoutes = routes;
            routes.forEach(route => {
                const lines = route.createPolylines();
                dayRouteLines.push(...lines);
                lines.forEach(line => line.setMap(map));
            });
            summary.textContent = `${rows.length} stops · ${distance(routes.reduce((total, route) => total + route.distanceMeters, 0))} total driving`;
            fit();
        } catch {
            if (version !== dayDistanceVersion) return;
            removeDayRoute();
            summary.textContent = 'Driving distance unavailable. Check your connection and try again.';
            el('retry-day-distance').hidden = false;
        }
    }
    function removeRoute() {
        routeLines.forEach(line => line.setMap(null));
        routeLines = [];
        drawnRoute = null;
    }
    function selectedRows() {
        const rows = [...itinerary.querySelectorAll('[data-stop-id]')];
        return selectedStops.map(id => rows.find(row => row.dataset.stopId === id)).filter(Boolean);
    }
    function updateSelection() {
        itinerary.querySelectorAll('.route-stop').forEach(input => {
            const row = input.closest('[data-stop-id]');
            const index = selectedStops.indexOf(row.dataset.stopId);
            const role = index === 0 ? 'A, origin' : 'B, destination';
            input.checked = index >= 0;
            input.disabled = selectedStops.length === 2 && !input.checked;
            input.setAttribute('aria-label', input.checked
                ? `${role}: ${row.dataset.stopName}. Deselect for route comparison`
                : `Select ${row.dataset.stopName} for route comparison`);
            const badge = row.querySelector('.stop-route-badge');
            badge.hidden = !input.checked;
            const letter = input.checked ? ['A', 'B'][index] : '';
            // Keep repeated renders from triggering the itinerary observer again.
            if (badge.textContent !== letter) badge.textContent = letter;
            badge.title = input.checked ? role : '';
        });
        markers.forEach(entry => {
            const index = selectedStops.indexOf(entry.row.dataset.stopId);
            entry.pin.classList.toggle('route-endpoint', index >= 0);
            entry.pin.querySelector('span').textContent = index < 0 ? entry.label : ['A', 'B'][index];
        });
        const rows = selectedRows();
        el('route-origin').textContent = rows[0]?.dataset.stopName || 'Select a starting stop';
        el('route-destination').textContent = rows[1]?.dataset.stopName || 'Select a destination';
        clearRouteButton.hidden = !rows.length;
        el('swap-route').disabled = rows.length !== 2;
    }
    function resetResults() {
        removeRoute();
        results = {};
        el('route-details').hidden = true;
        el('route-details').open = false;
        el('route-steps').replaceChildren();
        el('route-warnings').hidden = true;
        el('retry-route').hidden = true;
        modes.forEach(mode => {
            el(`time-${mode.toLowerCase()}`).textContent = '—';
            el(`route-${mode.toLowerCase()}`).disabled = selectedStops.length !== 2;
        });
    }
    function clearRoute() {
        ++routeVersion;
        selectedStops = [];
        resetResults();
        updateSelection();
        routeSummary.textContent = '';
        routeSummary.hidden = true;
    }
    function addDetail(text) {
        const item = document.createElement('li');
        item.textContent = text;
        el('route-steps').appendChild(item);
    }
    function showDetails(route) {
        el('route-steps').replaceChildren();
        if (activeMode === 'TRANSIT') {
            const steps = (route.legs || []).flatMap(leg => leg.steps || []);
            let boardings = 0;
            let walking = [];
            const flushWalking = destination => {
                if (!walking.length) return;
                const distances = walking.map(step => step.distanceMeters);
                const times = walking.map(step => step.staticDurationMillis);
                const length = distances.every(Number.isFinite) ? ` · ${distance(distances.reduce((sum, value) => sum + value, 0))}` : '';
                // Round once for the entire walking connection, not once per street instruction.
                const time = times.every(Number.isFinite) ? ` · ${duration(times.reduce((sum, value) => sum + value, 0))}` : '';
                addDetail(`Walk${destination ? ` to ${destination}` : ''}${length}${time}`);
                walking = [];
            };
            const clockTime = value => {
                const date = new Date(value);
                return Number.isFinite(date.getTime()) ? date.toLocaleTimeString([], {hour:'2-digit',minute:'2-digit'}) : '';
            };
            steps.forEach(step => {
                const transit = step.transitDetails;
                if (transit) {
                    const from = transit.departureStop?.name || 'boarding stop';
                    const to = transit.arrivalStop?.name || 'alighting stop';
                    flushWalking(from);
                    boardings++;
                    const line = transit.transitLine;
                    const label = line?.shortName || line?.name;
                    const departure = transit.departureTime ? clockTime(transit.departureTime) : '';
                    const arrival = transit.arrivalTime ? clockTime(transit.arrivalTime) : '';
                    const timing = departure && arrival ? ` · ${departure}–${arrival}` : departure ? ` · Departs ${departure}` : '';
                    const direction = transit.headsign ? ` (towards ${transit.headsign})` : '';
                    addDetail(`${label ? `Bus ${label}` : 'Bus'}${direction}: ${from} → ${to}${timing}`);
                } else if (step.travelMode === 'WALKING') {
                    walking.push(step);
                } else {
                    flushWalking('');
                }
            });
            flushWalking('destination');
            if (boardings) {
                const transfers = boardings - 1;
                addDetail(`${transfers ? `${transfers} transfer${transfers === 1 ? '' : 's'}` : 'No transfers'} · Bus times shown in ${timezone}.`);
            } else {
                addDetail('Google did not return bus boarding details for this route.');
            }
        } else if (activeMode === 'DRIVING') {
            addDetail('Estimate includes Google’s predicted traffic at departure.');
            if (Number.isFinite(route.staticDurationMillis)) addDetail(`Without traffic: ${duration(route.staticDurationMillis)}.`);
        } else {
            addDetail(`Walking distance: ${distance(route.distanceMeters)}.`);
        }
        const warnings = [...(route.warnings || [])];
        if (activeMode === 'WALKING') warnings.unshift('Walking routes are in beta and may be missing sidewalks or pedestrian paths.');
        el('route-warnings').textContent = warnings.join(' ');
        el('route-warnings').hidden = !warnings.length;
        el('route-details').hidden = false;
    }
    function showMode() {
        modes.forEach(mode => el(`route-${mode.toLowerCase()}`).setAttribute('aria-pressed', String(mode === activeMode)));
        const result = results[activeMode];
        el('route-details').hidden = true;
        el('route-warnings').hidden = true;
        el('retry-route').hidden = result?.state !== 'error';
        if (!result) return;
        if (result.state !== 'ready') {
            removeRoute();
            routeSummary.textContent = result.state === 'loading' ? `Finding a ${modeNames[activeMode].toLowerCase()} route…`
                : result.state === 'empty' ? `No ${modeNames[activeMode].toLowerCase()} route available for these stops and departure time.`
                : result.message;
            return;
        }
        const route = result.route;
        routeSummary.textContent = `${modeNames[activeMode]} · ${duration(route.durationMillis)} · ${distance(route.distanceMeters)}`;
        showDetails(route);
        if (drawnRoute === route) return;
        removeRoute();
        routeLines = route.createPolylines();
        routeLines.forEach(line => line.setMap(map));
        drawnRoute = route;
        const bounds = new google.maps.LatLngBounds();
        route.path.forEach(point => bounds.extend(point));
        const overlay = el('route-overlay');
        const narrow = window.matchMedia('(max-width: 600px)').matches;
        map.fitBounds(bounds, narrow ? 40 : { top:40, right:40, bottom:40, left:Math.min(overlay.offsetWidth + 32, panel.clientWidth / 2) });
    }
    async function withTimeout(work) {
        let timer;
        try {
            return await Promise.race([work(), new Promise((_, reject) => {
                timer = setTimeout(() => reject(new Error('Request timed out')), 20000);
            })]);
        } finally { clearTimeout(timer); }
    }
    async function requestRoute() {
        const version = ++routeVersion;
        resetResults();
        updateSelection();
        const rows = selectedRows();
        routeSummary.hidden = rows.length !== 2;
        if (rows.length !== 2) {
            routeSummary.textContent = '';
            return;
        }
        const departure = confirmedDeparture;
        if (departure && departure.getTime() <= Date.now()) {
            routeSummary.textContent = 'Your departure time has passed. Choose a new time or Use now.';
            return;
        }
        if (!map || failed) {
            routeSummary.textContent = failed ? 'The map is unavailable. Reload and try again.' : 'Waiting for the map to load…';
            return;
        }
        infoWindow.close();
        modes.forEach(mode => {
            results[mode] = { state:'loading' };
            el(`time-${mode.toLowerCase()}`).textContent = 'Loading…';
        });
        showMode();
        const libraries = Promise.all([google.maps.importLibrary('routes'), google.maps.importLibrary('places')]);
        await Promise.all(modes.map(async mode => {
            try {
                const response = await withTimeout(async () => {
                    const [{ Route }, { Place }] = await libraries;
                    if (version !== routeVersion) return null;
                    if (mode === 'TRANSIT' && departure && departure.getTime() - Date.now() > 100 * 86400000)
                        return { routes:[] };
                    const endpoints = rows.map(row => row.dataset.placeId ? new Place({id:row.dataset.placeId}) : coordinates(row.dataset.latlng));
                    if (endpoints.some(value => !value)) throw new Error('Missing location');
                    const request = {
                        origin:endpoints[0], destination:endpoints[1], travelMode:mode,
                        fields:['path','durationMillis','distanceMeters','warnings'], language:'en'
                    };
                    if (mode === 'DRIVING') {
                        request.routingPreference = 'TRAFFIC_AWARE';
                        request.fields.push('staticDurationMillis');
                    }
                    if (mode === 'TRANSIT') {
                        request.transitPreference = {allowedTransitModes:['BUS']};
                        request.fields.push('legs');
                    }
                    if (departure && mode !== 'WALKING') request.departureTime = departure;
                    return Route.computeRoutes(request);
                });
                if (version !== routeVersion) return;
                const route = response?.routes?.[0];
                if (route && (!Number.isFinite(route.durationMillis) || !Number.isFinite(route.distanceMeters) || !route.path?.length))
                    throw new Error('Incomplete route response');
                results[mode] = route ? {state:'ready',route} : {state:'empty'};
                el(`time-${mode.toLowerCase()}`).textContent = route ? duration(route.durationMillis) : 'No route';
            } catch (error) {
                if (version !== routeVersion) return;
                console.error(`Google ${mode} route failed:`, error);
                results[mode] = {state:'error',message:'Could not load this route. Retry, or check your connection and Google Routes API access.'};
                el(`time-${mode.toLowerCase()}`).textContent = 'Unavailable';
            }
            if (version === routeVersion && activeMode === mode) showMode();
        }));
    }
    itinerary.addEventListener('change', event => {
        if (!event.target.matches('.route-stop')) return;
        const checked = event.target.checked;
        if (previewDayId !== null) previewDay(null);
        const id = event.target.closest('[data-stop-id]').dataset.stopId;
        if (checked && selectedStops.length < 2) selectedStops.push(id);
        else selectedStops = selectedStops.filter(value => value !== id);
        if (!selectedStops.length) clearRoute(); else requestRoute();
    });
    modes.forEach(mode => el(`route-${mode.toLowerCase()}`).addEventListener('click', () => {
        activeMode = mode;
        el('route-details').open = false;
        showMode();
    }));
    el('swap-route').addEventListener('click', () => { selectedStops.reverse(); requestRoute(); });
    clearRouteButton.addEventListener('click', () => { clearRoute(); fit(); });
    el('retry-route').addEventListener('click', requestRoute);
    function closeDeparture() {
        el('route-schedule').hidden = true;
        departureToggle.setAttribute('aria-expanded', 'false');
        departureToggle.focus();
    }
    function applyDeparture(value) {
        confirmedDeparture = value;
        departureToggle.textContent = value
            ? `Depart ${value.toLocaleString([], {month:'short', day:'numeric', hour:'2-digit', minute:'2-digit'})} ▾`
            : 'Depart now ▾';
        closeDeparture();
        if (selectedStops.length === 2) requestRoute();
    }
    departureToggle.addEventListener('click', () => {
        if (!el('route-schedule').hidden) { closeDeparture(); return; }
        const initial = confirmedDeparture || new Date(Date.now() + 3600000);
        const local = new Date(initial.getTime() - initial.getTimezoneOffset() * 60000);
        departureInput.value = local.toISOString().slice(0, 16);
        el('departure-error').hidden = true;
        el('route-schedule').hidden = false;
        departureToggle.setAttribute('aria-expanded', 'true');
        departureInput.focus();
    });
    el('apply-departure').addEventListener('click', () => {
        const value = new Date(departureInput.value);
        if (!departureInput.value || !Number.isFinite(value.getTime()) || value.getTime() <= Date.now()) {
            el('departure-error').textContent = 'Choose a future date and time.';
            el('departure-error').hidden = false;
            return;
        }
        applyDeparture(value);
    });
    el('departure-now').addEventListener('click', () => applyDeparture(null));
    el('cancel-departure').addEventListener('click', closeDeparture);
    departureInput.addEventListener('input', () => { el('departure-error').hidden = true; });
    el('route-schedule').addEventListener('keydown', event => {
        if (event.key === 'Escape') { event.preventDefault(); closeDeparture(); }
        if (event.key === 'Enter' && event.target === departureInput) {
            event.preventDefault(); el('apply-departure').click();
        }
    });

    function showError(message) {
        failed = true;
        removeDayRoute();
        ++dayDistanceVersion;
        if (previewDayId !== null) el('day-preview-distance').textContent = 'Driving distance unavailable while the map is not connected.';
        clearRoute();
        routeSummary.hidden = false;
        routeSummary.textContent = message;
        setMapStatus(message);
    }
    function coordinates(value) {
        const parts = (value || '').split('_');
        if (parts.length !== 2 || parts.some(part => !part.trim())) return null;
        const [lat, lng] = parts.map(Number);
        return Number.isFinite(lat) && Number.isFinite(lng) && Math.abs(lat) <= 90 && Math.abs(lng) <= 180
            ? { lat, lng } : null;
    }
    function select(entry, scrollToStop) {
        itinerary.querySelectorAll('[data-stop-id]').forEach(row => {
            row.classList.toggle('map-selected', row === entry.row);
        });
        map.panTo(entry.position);
        map.setZoom(15);
        const label = document.createElement('div');
        label.style.color = '#1f2937';
        label.textContent = entry.marker.title;
        infoWindow.setContent(label);
        infoWindow.open({ map, anchor: entry.marker });
        if (scrollToStop) entry.row.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        else if (window.matchMedia('(max-width: 900px)').matches) panel.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
    function fit() {
        if (!map || (!markers.length && !dayRoutes.length)) return;
        infoWindow.close();
        if (markers.length === 1 && !dayRoutes.length && previewDayId === null) {
            map.setCenter(markers[0].position);
            map.setZoom(14);
            return;
        }
        const bounds = new google.maps.LatLngBounds();
        markers.forEach(entry => bounds.extend(entry.position));
        dayRoutes.forEach(route => route.path.forEach(point => bounds.extend(point)));
        const previewOverlay = el('day-preview');
        const padding = previewDayId !== null && !window.matchMedia('(max-width: 600px)').matches
            ? {top:40, right:40, bottom:40, left:Math.min(previewOverlay.offsetWidth + 32, panel.clientWidth / 2)} : 60;
        map.fitBounds(bounds, padding);
        google.maps.event.addListenerOnce(map, 'idle', () => {
            if (map.getZoom() > 16) map.setZoom(16);
        });
    }
    function render() {
        if (!map || failed) return;
        infoWindow.close();
        markers.forEach(entry => entry.marker.map = null);
        markers = [];
        let skipped = 0;
        itinerary.querySelectorAll('.stop-list').forEach((list, dayIndex) => {
            if (previewDayId !== null && list.dataset.dayId !== previewDayId) return;
            list.querySelectorAll('[data-stop-id]').forEach((row, stopIndex) => {
                const position = coordinates(row.dataset.latlng);
                if (!position) { skipped++; return; }
                const label = `${dayIndex + 1}.${stopIndex + 1}`;
                const pin = document.createElement('div');
                pin.className = 'trip-pin';
                const text = document.createElement('span');
                text.textContent = label;
                pin.appendChild(text);
                const marker = new Marker({
                    map, position, content: pin,
                    title: `${list.dataset.dayName || `Day ${dayIndex + 1}`}, stop ${stopIndex + 1}: ${row.dataset.stopName}`
                });
                const entry = { marker, row, position, pin, label };
                marker.addListener('click', () => select(entry, true));
                markers.push(entry);
            });
        });
        setMapStatus(skipped ? `${skipped} saved stop(s) have no valid coordinates.` : '');
        updateSelection();
    }
    async function init() {
        if (failed || map) return;
        try {
            const [{ Map, InfoWindow }, { AdvancedMarkerElement }] = await Promise.all([
                google.maps.importLibrary('maps'), google.maps.importLibrary('marker')
            ]);
            if (failed) return;
            Marker = AdvancedMarkerElement;
            map = new Map(panel, {
                center: { lat: 20, lng: 0 }, zoom: 2,
                mapId: panel.dataset.mapId,
                mapTypeControl: false, streetViewControl: false,
                gestureHandling: 'cooperative'
            });
            infoWindow = new InfoWindow();
            render();
            fit();
            if (selectedStops.length) requestRoute();
            if (previewDayId !== null) loadDayDistance();
            new MutationObserver(() => render()).observe(itinerary, { childList: true, subtree: true });
        } catch (error) {
            console.error('Trip map failed:', error);
            showError('The map could not load. Your saved stops are still available on the left.');
        }
    }
    itinerary.addEventListener('click', event => {
        const preview = event.target.closest('.preview-day');
        if (preview) { previewDay(preview.dataset.dayId === previewDayId ? null : preview.dataset.dayId); return; }
        const button = event.target.closest('.stop-map-focus');
        if (!button) return;
        const row = button.closest('[data-stop-id]');
        if (previewDayId !== null && row.closest('.stop-list').dataset.dayId !== previewDayId) previewDay(null);
        const entry = markers.find(item => item.row === row);
        if (entry) select(entry, false);
        else if (!failed) setMapStatus(map ? 'This stop has no valid map coordinates.' : 'The map is still loading.');
    });
    itinerary.addEventListener('dragstart', event => {
        if (event.defaultPrevented || previewDayId === null || !event.target.closest('[data-stop-id]') ||
            event.target.closest('button, input, textarea, summary, details')) return;
        ++dayDistanceVersion;
        removeDayRoute();
        el('day-preview-distance').textContent = 'Updating stop order…';
        el('retry-day-distance').hidden = true;
    });
    itinerary.addEventListener('stop-order-settled', () => {
        if (previewDayId !== null) { render(); fit(); loadDayDistance(); }
    });
    el('retry-day-distance').addEventListener('click', loadDayDistance);
    window.tripMap = { init, showError };
})();
