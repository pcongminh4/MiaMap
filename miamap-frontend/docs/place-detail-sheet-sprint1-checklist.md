# Sprint 1 Checklist - Place Detail Experience

This checklist breaks Sprint 1 into concrete implementation tasks tied directly to files in the current frontend codebase.

## Goal

Ship a polished place-tap experience on the home map screen:
- stable marker selection
- clean place detail sheet
- strong fallback behavior for incomplete data
- consistent Vietnamese UI text

## Checklist

### 1. Place Detail Sheet Core

File:
- [PlaceDetailSheet.tsx](/C:/github-projects/MiaMap/miamap-frontend/src/features/home/components/PlaceDetailSheet.tsx)

Tasks:
- [x] Render a place detail bottom sheet when a map place is selected
- [x] Show primary content blocks: thumbnail, title, category, metadata, address, and CTAs
- [x] Support fallback thumbnail presentation when real images are missing
- [x] Support missing rating state
- [x] Support missing phone state with disabled `Gọi`
- [x] Support missing menu state with disabled `Thực đơn`
- [ ] Add a loading or skeleton variant for future async place detail loading
- [ ] Add richer category artwork if dedicated design assets are created later
- [ ] Add explicit saved-state feedback when the save flow exists

### 2. Marker Interaction

File:
- [HomeMap.tsx](/C:/github-projects/MiaMap/miamap-frontend/src/features/home/components/HomeMap.tsx)

Tasks:
- [x] Make place markers clickable
- [x] Highlight the selected marker
- [x] Prevent accidental open-close conflicts caused by click bubbling
- [x] Close the sheet when the user taps the map background
- [ ] Improve selected-marker animation or halo polish
- [ ] Tune behavior for dense clusters of markers if overlap becomes a UX issue

### 3. Home Screen Wiring

File:
- [home-page.tsx](/C:/github-projects/MiaMap/miamap-frontend/src/features/home/home-page.tsx)

Tasks:
- [x] Store the selected place in home screen state
- [x] Resolve the selected place from bounding-box results
- [x] Open the detail sheet from marker selection
- [x] Close the detail sheet on demand
- [x] Route to the selected place from the sheet CTA
- [ ] Add analytics or tracking hooks if product wants interaction telemetry

### 4. Vietnamese UI Cleanup

Files:
- [PlaceDetailSheet.tsx](/C:/github-projects/MiaMap/miamap-frontend/src/features/home/components/PlaceDetailSheet.tsx)
- [SearchPanel.tsx](/C:/github-projects/MiaMap/miamap-frontend/src/features/home/components/SearchPanel.tsx)
- [MapControls.tsx](/C:/github-projects/MiaMap/miamap-frontend/src/features/home/components/MapControls.tsx)
- [TopRightActions.tsx](/C:/github-projects/MiaMap/miamap-frontend/src/features/home/components/TopRightActions.tsx)
- [HomeMap.tsx](/C:/github-projects/MiaMap/miamap-frontend/src/features/home/components/HomeMap.tsx)
- [home-page.tsx](/C:/github-projects/MiaMap/miamap-frontend/src/features/home/home-page.tsx)

Tasks:
- [x] Replace plain ASCII Vietnamese labels with proper diacritics in the place sheet
- [x] Replace plain ASCII Vietnamese labels with proper diacritics in the home search panel
- [x] Replace plain ASCII Vietnamese labels with proper diacritics in map-related helper UI
- [ ] Audit any remaining Vietnamese text outside `home/` that still lacks diacritics

### 5. Visual Polish

Files:
- [PlaceDetailSheet.tsx](/C:/github-projects/MiaMap/miamap-frontend/src/features/home/components/PlaceDetailSheet.tsx)
- [HomeMap.tsx](/C:/github-projects/MiaMap/miamap-frontend/src/features/home/components/HomeMap.tsx)

Tasks:
- [x] Improve hierarchy of the sheet
- [x] Make CTA priority clearer
- [x] Make the fallback image block feel intentional
- [x] Add status badge styles
- [ ] Tune final spacing after visual QA in-browser
- [ ] Verify layout at smaller laptop widths
- [ ] Verify mobile behavior once the screen is tested responsively

## Validation

Recommended checks before closing Sprint 1:

- [ ] Tap several markers and confirm the correct place opens every time
- [ ] Confirm the selected marker remains obvious while the sheet is visible
- [ ] Confirm `Chỉ đường` pushes the selected place into the existing route flow
- [ ] Confirm the UI still looks complete when rating, phone, or menu are missing
- [ ] Confirm all visible Vietnamese strings in the home flow use proper diacritics
- [ ] Run lint and ensure no new lint errors were introduced by the feature work

## Known Existing Issue Outside Sprint 1

Current lint errors still exist in:
- [http.ts](/C:/github-projects/MiaMap/miamap-frontend/src/services/api/http.ts)

These are not caused by the place detail work and can be handled separately.
