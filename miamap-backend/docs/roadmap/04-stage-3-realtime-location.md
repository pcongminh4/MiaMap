# Stage 3 - Realtime Location and WebSocket Synchronization

## Goal

Manage realtime driver presence and location updates within District 1.

## Components To Add

- `[NEW]` [LocationHub.cs](/C:/github-projects/MiaMap/miamap-backend/Infrastructure/Realtime/LocationHub.cs)

## Main Responsibilities Of `LocationHub`

- Receive GPS updates from the client at regular intervals, expected every 3 to 5 seconds.
- Store coordinates in in-memory cache or Redis, associated with the user ID and mood avatar.
- Broadcast anonymized nearby user positions to relevant clients within a 1 km radius.

## Notes

- Privacy should be treated carefully, and exact coordinates should not be exposed more than necessary.
- The system should include timeout logic for disconnected or inactive sessions.
