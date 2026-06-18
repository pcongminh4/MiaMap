# Frontend Sprint Plan

This document defines the next short-term frontend plan for MiaMap based on the current state of the `home/` feature.

The plan is intentionally practical:
- improve the current map experience first
- stabilize the feature structure before adding more surface area
- only expand into reports and AI after the map interaction layer feels solid

## Current Frontend State

What already exists:
- map screen centered on District 1
- static route planning flow
- bounding-box place loading
- basic place selection on the map
- initial `PlaceDetailSheet` component
- cleaner `home/` structure with `components`, `hooks`, `queries`, `types`, and `utils`

Current gaps:
- several UI strings still need proper Vietnamese diacritics
- `PlaceDetailSheet` still uses partial mock data and visual fallback assets are basic
- map interactions are working but not yet polished
- server-state usage is decent but not yet fully standardized
- incident and AI flows are still roadmap-level, not implementation-ready

## Planning Principle

The next sprints should prioritize:

1. A better usable map experience
2. Cleaner frontend state boundaries
3. Stronger fallback behavior when backend data is incomplete
4. A stable interaction model before feature expansion

## Sprint 1: Place Detail Experience

### Goal

Turn the current place-tap experience into a polished and production-ready interaction.

### Scope

- Finish the `PlaceDetailSheet` UI
- Align the implementation with the approved design direction
- Improve selection, dismissal, and route-entry flow
- Make incomplete data look intentional rather than broken

### Tasks

- Refine [PlaceDetailSheet.tsx](/C:/github-projects/MiaMap/miamap-frontend/src/features/home/components/PlaceDetailSheet.tsx)
  - apply proper Vietnamese labels with diacritics
  - improve spacing, hierarchy, and button emphasis
  - make fallback image blocks more polished
  - improve status badge visuals
- Update [HomeMap.tsx](/C:/github-projects/MiaMap/miamap-frontend/src/features/home/components/HomeMap.tsx)
  - strengthen selected-marker highlight
  - improve marker click behavior in dense map areas
  - prevent accidental close/open conflicts
- Update [home-page.tsx](/C:/github-projects/MiaMap/miamap-frontend/src/features/home/home-page.tsx)
  - make place selection flow more explicit
  - ensure `Chỉ đường` transitions cleanly into destination routing
- Add loading and partial-data states
  - skeleton state
  - no-image state
  - no-rating state
  - no-phone state
  - no-menu state

### Definition of Done

- Tapping a place always opens a stable detail sheet
- The detail sheet looks complete even when some fields are missing
- The route CTA works reliably
- The sheet can be opened, switched, and dismissed smoothly

## Sprint 2: Home Screen UI Consistency and State Cleanup

### Goal

Make the `home/` feature visually consistent and easier to maintain.

### Scope

- Finish UI language cleanup
- reduce friction between UI state and server state
- keep the feature easy to extend before adding incidents or AI

### Tasks

- Normalize Vietnamese UI text across:
  - [SearchPanel.tsx](/C:/github-projects/MiaMap/miamap-frontend/src/features/home/components/SearchPanel.tsx)
  - [MapControls.tsx](/C:/github-projects/MiaMap/miamap-frontend/src/features/home/components/MapControls.tsx)
  - [TopRightActions.tsx](/C:/github-projects/MiaMap/miamap-frontend/src/features/home/components/TopRightActions.tsx)
  - [HomeMap.tsx](/C:/github-projects/MiaMap/miamap-frontend/src/features/home/components/HomeMap.tsx)
- Audit server state usage
  - keep TanStack in `queries/`
  - ensure local UI state remains in `hooks/`
  - identify any server data still being shaped too late in components
- Improve query ergonomics
  - review `staleTime`
  - add `select` where useful
  - evaluate placeholder behavior for smoother transitions
- Reduce coupling in the `home/` feature
  - split any growing component that becomes too large
  - keep orchestration in hooks, not in presentational components

### Definition of Done

- Core home UI uses consistent Vietnamese wording
- `home/` remains readable after adding place-detail interaction
- Server-state and UI-state boundaries are easier to reason about

## Sprint 3: Incidents Foundation

### Goal

Prepare the frontend foundation for crowdsourced incident reporting without overbuilding.

### Scope

- create the first usable UX flow for reporting incidents
- establish incident rendering rules on the map
- avoid building the full social layer too early

### Tasks

- Create [ReportMenuModal.tsx](/C:/github-projects/MiaMap/miamap-frontend/src/features/home/components/ReportMenuModal.tsx)
  - one-tap or two-tap incident reporting UX
  - large accessible touch targets
  - category-first layout
- Extend [HomeMap.tsx](/C:/github-projects/MiaMap/miamap-frontend/src/features/home/components/HomeMap.tsx)
  - add incident markers
  - add urgent marker styling
  - support incident popups or light detail surfaces
- Add frontend mutations for incidents
  - report creation
  - quick vote actions such as `Still there` and `Gone`
- Define incident UI fallback rules
  - no description
  - unknown freshness
  - low-confidence report

### Definition of Done

- A user can open an incident menu and submit a basic report
- Incident markers can appear on the map in a recognizable way
- The frontend structure is ready for backend incident APIs

## Optional Sprint 4: AI Entry Surface

### Goal

Introduce the UI shell for the AI assistant without fully depending on completed backend intelligence.

### Scope

- create the panel
- define message layout
- prepare map-action integration

### Tasks

- Create [AIAgentPanel.tsx](/C:/github-projects/MiaMap/miamap-frontend/src/features/home/components/AIAgentPanel.tsx)
- Support simple assistant messages
- Prepare interaction hooks for:
  - zoom to places
  - route to place
  - highlight suggested locations

### Definition of Done

- The AI panel can be mounted into the home screen cleanly
- The frontend can consume structured AI responses later without major layout changes

## Recommended Order

If the team wants the safest sequence, use this order:

1. Sprint 1: Place detail experience
2. Sprint 2: Home screen consistency and state cleanup
3. Sprint 3: Incidents foundation
4. Sprint 4: AI entry surface

## Notes for Implementation

- Do not wait for perfect backend data before improving the UX
- Build robust fallback behavior early
- Keep the `home/` feature modular as interaction density grows
- Prefer smooth user experience over adding many shallow features quickly
