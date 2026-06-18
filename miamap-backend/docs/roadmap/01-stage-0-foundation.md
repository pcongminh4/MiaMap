# Stage 0 - Foundation and Static Routing

## Goal

Set up the core geospatial database structure and static route-finding for District 1, Ho Chi Minh City.

## Status

`COMPLETED`

## Implementation Details

- `[DONE]` Set up the database schema with `nodes`, `roads`, `places`, and `users`.
- `[DONE]` Imported OpenStreetMap data filtered to District 1.
- `[DONE]` Built static Dijkstra routing in [RoutingRepository.cs](/C:/github-projects/MiaMap/miamap-backend/Infrastructure/Places/RoutingRepository.cs).
- `[DONE]` Exposed `GET /places/route` to return a list of `GeoPoint` values for route drawing.

## Technical Notes

- This stage does not account for traffic, live speed, or time-based factors yet.
- Current routing weights are primarily based on geographic distance.
