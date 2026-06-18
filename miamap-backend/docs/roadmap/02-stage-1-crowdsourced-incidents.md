# Stage 1 - Crowdsourced Incidents

## Goal

Build APIs that allow users to report traffic incidents in real time within District 1.

## Authentication Decision

This stage requires authentication for actions that change community data.

- `GET /reports/active`:
  Authentication is not required. This map data should be broadly visible to all users.
- `POST /reports`:
  Authentication is required. Every report must be tied to its creator for moderation, anti-spam, and gamification.
- `POST /reports/{id}/vote`:
  Authentication is required. Votes must be tied to users to prevent abuse and duplicate voting.

## Auth Dependency

Stage 1 should not be treated as fully independent from authentication.

- The backend already has basic JWT auth with login and registration.
- Before Stage 1 is considered complete, report creation and voting APIs must be able to resolve `CurrentUserId` from the access token.
- If authentication is not production-ready yet, it should be completed before opening report creation to users.

## Required Auth Scope For Stage 1

- Stable registration and login endpoints.
- Consistent JWT claims containing a user identifier.
- Authorization on report creation and voting endpoints.
- An abstraction for resolving the current user in the application layer or endpoint layer.
- Rate limiting and basic anti-spam rules for report creation.

## Incident Types

- Traffic jam
- Police
- Accident
- Hazard

## Database

### `reports` Table

- `Id` (int, PK)
- `CreatedByUserId` (int, FK to `users`)
- `ReportType` (nvarchar)
- `SubType` (nvarchar)
- `Location` (Geometry Point, SRID 4326)
- `Description` (nvarchar)
- `Upvotes` (int)
- `Downvotes` (int)
- `CreatedAtUtc` (datetime)
- `ExpiresAtUtc` (datetime)
- `IsActive` (bit)

## Components To Add

- `[NEW]` [Report.cs](/C:/github-projects/MiaMap/miamap-backend/Domain/Places/Report.cs)
  Domain entity that defines expiration rules, for example a traffic jam report may live for 45 minutes.
- `[NEW]` [CreateReportCommand.cs](/C:/github-projects/MiaMap/miamap-backend/Application/Places/CreateReport/CreateReportCommand.cs)
  Accepts coordinates and creates a new report. This command should not accept `CreatedByUserId` from the client directly, and should instead use the authenticated user.
- `[NEW]` [GetActiveReportsQuery.cs](/C:/github-projects/MiaMap/miamap-backend/Application/Places/GetActiveReports/GetActiveReportsQuery.cs)
  Returns active incidents inside the current map bounding box. This endpoint can remain public.
- `[NEW]` [VoteReportCommand.cs](/C:/github-projects/MiaMap/miamap-backend/Application/Places/VoteReport/VoteReportCommand.cs)
  Handles upvote and downvote actions to improve reliability or hide stale incidents. This requires authorization and vote tracking per user.

## Suggested Work Breakdown For Stage 1

1. Finalize the auth contract for report creation and voting.
2. Add an abstraction for resolving the current user ID from the request context.
3. Create the `reports` schema and its foreign key constraints to `users`.
4. Implement `CreateReportCommand` and an authorized report creation endpoint.
5. Implement `GetActiveReportsQuery` for the map client.
6. Implement `VoteReportCommand` with duplicate-vote protection.
