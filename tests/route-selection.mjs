import assert from 'node:assert/strict';
const listeners = {};
const classes = {toggle(){},add(){},remove(){}};
const make = () => ({textContent:'',hidden:false,disabled:false,value:'',style:{},classList:classes,children:[],offsetWidth:340,clientWidth:900,
    focus(){},setAttribute(key,value){this[key]=value;},replaceChildren(){this.children=[];},appendChild(child){this.children.push(child);},
    querySelector(){return this.children[0];},addEventListener(type,fn){this[type]=fn;}});
const badges = [make(),make(),make()];
const rows = [1,2,3].map(id=>({dataset:{stopId:String(id),stopName:'Stop '+id,placeId:'place-'+id,latlng:'43_-80'},classList:classes,querySelector(){return badges[id-1];}}));
const checks = rows.map(row=>({...make(),checked:false,closest(){return row;},matches(){return true;}}));
const list = {dataset:{dayId:'1',dayName:'Test day'},rows:[...rows],querySelectorAll(){return this.rows;}};
const secondList = {dataset:{dayId:'2',dayName:'Another day'},rows:[],querySelectorAll(){return this.rows;}};
const lists = [list,secondList];
const previewButtons = lists.map(list=>({...make(),dataset:{dayId:list.dataset.dayId}}));
const itinerary = {querySelectorAll(selector){return selector==='.route-stop'?checks:selector==='.stop-list'?lists:selector==='.preview-day'?previewButtons:rows;},addEventListener(type,fn){listeners[type]=fn;}};
const ids = ['trip-map','map-status','route-summary','clear-route','route-timezone','route-departure-toggle','departure-error','departure-now','cancel-departure','route-departure-time','route-details','route-steps','route-warnings','retry-route','route-origin','route-destination','swap-route','route-overlay','apply-departure','route-schedule',...['driving','transit','walking'].flatMap(mode=>['route-'+mode,'time-'+mode])];
const elements = Object.fromEntries(ids.map(id=>[id,make()]));
for(const id of ['day-preview','day-preview-name','day-preview-distance','retry-day-distance','day-route-stops']) elements[id]=make();
elements['trip-map'].dataset={mapId:'test'};elements['trip-days']=itinerary;elements['route-schedule'].hidden=true;
globalThis.document={getElementById:id=>elements[id],createElement:make};
globalThis.window={matchMedia(){return {matches:false};}};
globalThis.MutationObserver=class{observe(){}};
let requests=[],pending=[],drawn=[];
let lastBounds, lastPadding;
class FakeMap{setCenter(){}setZoom(){}getZoom(){return 12;}fitBounds(bounds,padding){lastBounds=bounds;lastPadding=padding;}panTo(){}}
const allMarkers=[];
class FakeMarker{constructor(options){Object.assign(this,options);allMarkers.push(this);}addListener(){}}
globalThis.google={maps:{LatLngBounds:class{constructor(){this.points=[];}extend(point){this.points.push(point);}},event:{addListenerOnce(){}},importLibrary:async name=>({
    maps:{Map:FakeMap,InfoWindow:class{close(){}setContent(){}open(){}}},marker:{AdvancedMarkerElement:FakeMarker},
    places:{Place:class{constructor(options){Object.assign(this,options);}}},
    routes:{Route:{computeRoutes(request){requests.push(request);return new Promise((resolve,reject)=>pending.push({mode:request.travelMode,resolve,reject}));}}}
})[name]}};
await import('../wwwroot/js/trip-map.js');
await window.tripMap.init();
const tick=()=>new Promise(resolve=>setTimeout(resolve,0));
const choose=async(index,value)=>{checks[index].checked=value;listeners.change({target:checks[index]});await tick();};
const click=async id=>{elements[id].click();await tick();};
const result=(mode)=>({routes:[{durationMillis:5400000,distanceMeters:12000,staticDurationMillis:3600000,path:[{lat:43,lng:-80}],warnings:mode==='WALKING'?['Test walking warning']:[],legs:mode==='TRANSIT'?[{steps:[{travelMode:'WALKING',distanceMeters:250},{transitDetails:{transitLine:{shortName:'7'},departureStop:{name:'A station'},arrivalStop:{name:'B station'}}}]}]:[],createPolylines(){return [{setMap(value){drawn.push({mode,value});}}];}}]});
const resolve=async mode=>{const index=pending.findIndex(item=>item.mode===mode);const response=result(mode);
if(mode==='TRANSIT') response.routes[0].legs[0].steps=[
    ...[48,81,286,133,6].map(distanceMeters=>({travelMode:'WALKING',distanceMeters,staticDurationMillis:20000})),
    {transitDetails:{transitLine:{shortName:'30'},departureStop:{name:'Kitchener GO'},arrivalStop:{name:'University of Waterloo Terminal'},departureTime:new Date('2026-09-21T21:53:00Z'),arrivalTime:new Date('2026-09-21T22:13:00Z'),headsign:'U of Waterloo'}},
    ...[66,89,73].map(distanceMeters=>({travelMode:'WALKING',distanceMeters,staticDurationMillis:20000}))
];
pending.splice(index,1)[0].resolve(response);await tick();};
await choose(0,true);assert.equal(requests.length,0);
assert.equal(badges[0].textContent,'A');assert.equal(badges[0].hidden,false);assert.equal(badges[1].hidden,true);
await choose(2,true);assert.equal(requests.length,3);assert.ok(checks[1].disabled);
assert.equal(badges[2].textContent,'B');assert.equal(badges[2].hidden,false);assert.match(checks[2]['aria-label'],/B, destination/);
for(const request of requests){assert.equal(request.origin.id,'place-1');assert.equal(request.destination.id,'place-3');assert.equal(request.intermediates,undefined);}
assert.equal(requests[0].routingPreference,'TRAFFIC_AWARE');assert.equal(requests[1].routingPreference,undefined);assert.deepEqual(requests[1].transitPreference,{allowedTransitModes:['BUS']});assert.equal(requests[2].routingPreference,undefined);
await resolve('WALKING');assert.match(elements['time-walking'].textContent,/1 hr 30 min/);assert.equal(drawn.length,0);
await resolve('DRIVING');assert.match(elements['route-summary'].textContent,/Car.*1 hr 30 min.*12.0 km/);
await resolve('TRANSIT');const count=requests.length;
await click('route-transit');assert.equal(requests.length,count);const details=elements['route-steps'].children.map(x=>x.textContent);
assert.equal(details.length,4);
assert.equal(details[0],'Walk to Kitchener GO · 554 m · 2 min');
assert.match(details[1],/Bus 30.*Kitchener GO → University of Waterloo Terminal/);
assert.equal(details[2],'Walk to destination · 228 m · 1 min');
assert.match(details[3],/No transfers/);
await click('route-walking');assert.equal(requests.length,count);assert.match(elements['route-warnings'].textContent,/Test walking warning/);assert.equal(elements['route-walking']['aria-pressed'],'true');
await click('swap-route');assert.equal(requests.length,6);assert.equal(requests.at(-1).origin.id,'place-3');
assert.equal(badges[0].textContent,'B');assert.equal(badges[2].textContent,'A');
await click('clear-route');pending.splice(0).forEach(item=>item.resolve(result(item.mode)));await tick();assert.equal(elements['route-summary'].hidden,true);assert.ok(checks.every(x=>!x.checked&&!x.disabled));assert.equal(drawn.at(-1).value,null);
assert.ok(badges.every(badge=>badge.hidden&&badge.textContent===''));
await choose(0,true);await choose(1,true);pending.find(x=>x.mode==='TRANSIT').resolve({routes:[]});pending.find(x=>x.mode==='DRIVING').reject(new Error('API disabled'));pending.find(x=>x.mode==='WALKING').resolve(result('WALKING'));pending=[];await tick();
await click('route-transit');assert.match(elements['route-summary'].textContent,/No bus route/);
await click('route-driving');assert.equal(elements['retry-route'].hidden,false);
await click('route-walking');assert.match(elements['route-summary'].textContent,/Walk/);
const before=requests.length;
await click('route-departure-toggle');assert.equal(elements['route-schedule'].hidden,false);assert.equal(requests.length,before);
elements['route-departure-time'].value='2000-01-01T10:00';await click('apply-departure');assert.equal(requests.length,before);assert.equal(elements['departure-error'].hidden,false);
await click('cancel-departure');assert.equal(elements['route-schedule'].hidden,true);assert.equal(requests.length,before);
await click('route-departure-toggle');
const future=new Date(Date.now()+86400000);future.setMinutes(future.getMinutes()-future.getTimezoneOffset());elements['route-departure-time'].value=future.toISOString().slice(0,16);await click('apply-departure');assert.equal(requests.length,before+3);assert.equal(elements['route-schedule'].hidden,true);
assert.ok(requests.at(-3).departureTime instanceof Date);assert.ok(requests.at(-2).departureTime instanceof Date);assert.equal(requests.at(-1).departureTime,undefined);
pending.splice(0).forEach(item=>item.resolve(result(item.mode)));await tick();
await click('route-departure-toggle');elements['route-departure-time'].value='';elements['route-departure-time'].input();await click('cancel-departure');assert.equal(requests.length,before+3);
await click('route-departure-toggle');await click('departure-now');assert.equal(elements['route-departure-toggle'].textContent,'Depart now ▾');assert.equal(requests.at(-3).departureTime,undefined);assert.equal(requests.at(-2).departureTime,undefined);
pending.splice(0).forEach(item=>item.resolve(result(item.mode)));await tick();
await click('clear-route');
await choose(0,true);await choose(2,true);
pending.splice(0).forEach(item=>item.resolve(result(item.mode)));await tick();
await choose(0,false);
assert.equal(badges[0].hidden,true);assert.equal(badges[2].textContent,'A');assert.equal(badges[2].hidden,false);
assert.match(checks[2]['aria-label'],/A, origin/);
await click('clear-route');
console.log('PASS: route comparison, A/B badges (selection, swap, deselection, clear), and compact departure picker.');

// Day preview uses every adjacent pair, never a shortcut from first to last.
const preview=async index=>{listeners.click({target:{closest(selector){return selector==='.preview-day'?previewButtons[index]:null;}}});await tick();};
const distanceText=()=>elements['day-preview-distance'].textContent;
const visiblePins=()=>allMarkers.filter(marker=>marker.map);
const dayLines=[];
const dayResult=meters=>({routes:[{distanceMeters:meters,path:[{lat:44,lng:-81},{lat:45,lng:-82}],createPolylines(){const line={map:null,setMap(value){this.map=value;}};dayLines.push(line);return [line];}}]});
const previewStart=requests.length;
await preview(0);
assert.equal(elements['route-overlay'].hidden,true);
assert.equal(elements['day-preview'].hidden,false);
assert.equal(visiblePins().length,3);
assert.equal(previewButtons[0]['aria-pressed'],'true');
assert.equal(requests.length,previewStart+2);
assert.deepEqual(requests.slice(-2).map(r=>[r.origin.id,r.destination.id]),[['place-1','place-2'],['place-2','place-3']]);
assert.ok(requests.slice(-2).every(r=>r.travelMode==='DRIVING'&&r.fields.includes('distanceMeters')&&r.fields.includes('path')));
pending.shift().resolve(dayResult(1500));
pending.shift().resolve(dayResult(2500));await tick();
assert.match(distanceText(),/3 stops · 4.0 km total driving/);
assert.equal(dayLines.filter(line=>line.map).length,2);
assert.deepEqual(elements['day-route-stops'].children.map(item=>item.children[0].children[1].textContent),['Stop 1','Stop 2','Stop 3']);
assert.equal(elements['day-route-stops'].children[0].children[0].children[0].textContent,'1.1');
assert.ok(lastBounds.points.some(point=>point.lat===45));assert.ok(lastPadding.left>60);
await preview(0);assert.equal(requests.length,previewStart+2);
assert.equal(elements['day-preview'].hidden,true);assert.equal(elements['route-overlay'].hidden,false);
assert.ok(dayLines.every(line=>line.map===null));assert.equal(previewButtons[0]['aria-pressed'],'false');
assert.equal(elements['map-status'].hidden,true);
await preview(0);assert.equal(requests.length,previewStart+2);assert.equal(dayLines.filter(line=>line.map).length,2);
// Only the selected day's markers remain; empty/single-stop days make no calls.
list.rows=[rows[0],rows[1]];secondList.rows=[rows[2]];
await preview(1);assert.equal(visiblePins().length,1);assert.equal(visiblePins()[0].content.children[0].textContent,'2.1');
assert.match(distanceText(),/1 stop · 0 km/);assert.ok(dayLines.every(line=>line.map===null));
secondList.rows=[];listeners['stop-order-settled']();await tick();assert.equal(visiblePins().length,0);assert.match(distanceText(),/No stops yet/);
assert.equal(requests.length,previewStart+2);
// Reorder refresh: a missing section must never produce a misleading partial total.
list.rows=[rows[2],rows[1],rows[0]];await preview(0);
assert.deepEqual(requests.slice(-2).map(r=>[r.origin.id,r.destination.id]),[['place-3','place-2'],['place-2','place-1']]);
pending.shift().resolve(dayResult(1200));pending.shift().resolve({routes:[]});await tick();
assert.match(distanceText(),/Total driving distance unavailable/);assert.ok(dayLines.every(line=>line.map===null));assert.match(distanceText(),/2 → 3/);
assert.equal(elements['retry-day-distance'].hidden,false);
const beforeRetry=requests.length;await click('retry-day-distance');assert.equal(requests.length,beforeRetry+1);
pending.shift().resolve(dayResult(800));await tick();assert.match(distanceText(),/2.0 km total driving/);
// Moving a stop to another day recalculates only the new adjacent sections.
list.rows=[rows[0],rows[2]];secondList.rows=[rows[1]];
listeners['stop-order-settled']();await tick();assert.equal(requests.at(-1).origin.id,'place-1');assert.equal(requests.at(-1).destination.id,'place-3');
assert.equal(visiblePins().length,2);
// Switching days ignores an older response, including errors.
await preview(1);pending.shift().reject(new Error('Unavailable'));await tick();assert.match(distanceText(),/1 stop · 0 km/);
await preview(1);assert.equal(elements['day-preview'].hidden,true);assert.equal(elements['route-overlay'].hidden,false);assert.equal(visiblePins().length,3);
assert.ok(previewButtons.every(button=>button['aria-pressed']==='false'));assert.ok(dayLines.every(line=>line.map===null));
// Selecting endpoints exits day preview and preserves the user's first checkbox.
await preview(1);await choose(0,true);assert.equal(elements['day-preview'].hidden,true);assert.equal(badges[0].textContent,'A');
assert.equal(visiblePins().length,3);await click('clear-route');
console.log('PASS: day preview visibility, adjacent driving totals, cache, empty days, reorder/move, unavailable routes, retry, stale responses, and A/B mode transition.');
