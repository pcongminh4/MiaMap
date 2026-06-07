import type { LatLngTuple } from 'leaflet'

export type BoundingBoxBounds = {
  minLatitude: number
  maxLatitude: number
  minLongitude: number
  maxLongitude: number
}

export const queryKeys = {
  searchPlaces: (searchText: string, limit: number) =>
    ['places', 'search', searchText.trim().toLowerCase(), limit] as const,
  route: (origin: LatLngTuple | null, destination: LatLngTuple | null) =>
    ['places', 'route', origin?.[0], origin?.[1], destination?.[0], destination?.[1]] as const,
  boundingBox: (bounds: BoundingBoxBounds | null, limit: number) =>
    ['places', 'bounding-box', bounds?.minLatitude, bounds?.maxLatitude, bounds?.minLongitude, bounds?.maxLongitude, limit] as const,
}
