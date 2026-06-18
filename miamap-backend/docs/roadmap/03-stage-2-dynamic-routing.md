# Stage 2 - Traffic Density and Dynamic Routing

## Goal

Extend routing so the system can automatically avoid heavily congested road segments in District 1.

## Components To Modify

- `[MODIFY]` [Road.cs](/C:/github-projects/MiaMap/miamap-backend/Domain/Places/Road.cs)
  Add `TrafficFactor` and `CurrentSpeed`.
- `[MODIFY]` [RoutingRepository.cs](/C:/github-projects/MiaMap/miamap-backend/Infrastructure/Places/RoutingRepository.cs)
  Scan active `jam` reports on affected road segments and update routing weights.
- `[MODIFY]` [FindRouteResult.cs](/C:/github-projects/MiaMap/miamap-backend/Application/Places/FindRoute/FindRouteResult.cs)
  Return `Segments` with traffic states so the frontend can render traffic colors.

## Weight Formula

```text
Weight = Road.Length * TrafficFactor
```

## Expected Behavior

- Congested roads should receive a higher `TrafficFactor`.
- Dijkstra should prefer a clearer route instead of only the geographically shortest one.
