# Stage 5 - Detailed Product Search and AI Agent Support

This document covers two major branches: semantic product search and an AI assistant for the map experience.

## Part A - Detailed Product Search

### Goal

Support searches for specific items inside a venue menu or service catalog, for example `Matcha Latte`.

### Database

- `[NEW]` Table `menu_items` or `products`
- Suggested columns:
  - `Id` (int, PK)
  - `PlaceId` (int, FK)
  - `Name` (nvarchar)
  - `Description` (nvarchar)
  - `Price` (decimal)
  - `Tags` (nvarchar)

### Components To Add Or Modify

- `[NEW]` [MenuItem.cs](/C:/github-projects/MiaMap/miamap-backend/Domain/Places/MenuItem.cs)
  Entity that stores a place's products or services.
- `[MODIFY]` [PlaceRepository.cs](/C:/github-projects/MiaMap/miamap-backend/Infrastructure/Places/PlaceRepository.cs)
  Add `SearchByProductAsync(string query, int limit)` using full-text search or vector search.

### Example Behavior

- The user types `matcha latte`.
- The system searches `menu_items`.
- The result is a list of matching `Place` records inside District 1.

## Part B - AI Agent Assistant On The Map

### Goal

Allow users to search and control the map using natural language.

### Components To Add

- `[NEW]` [AgentEndpoints.cs](/C:/github-projects/MiaMap/miamap-backend/Api/Endpoint/Agent/AgentEndpoints.cs)
  Provide the `POST /agent/chat` API.
- `[NEW]` [AgentService.cs](/C:/github-projects/MiaMap/miamap-backend/Application/Agent/AgentService.cs)
  Handle prompts, RAG orchestration, and response composition.

### Suggested RAG Flow

1. Search for cafes with parking and menu items containing `Matcha Latte` within a 2 km radius of the user.
2. Feed the retrieved results into the LLM as context.
3. Receive a natural-language answer plus a list of recommended places.

### Suggested JSON Response Shape

- `answer`: The AI assistant response text.
- `recommendedPlaces`: A list of suggested places with coordinates.
- `mapAction`: A recommended frontend action such as `zoom_to_places` or `draw_route`.
