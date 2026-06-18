import type { LatLngTuple } from 'leaflet'
import type { SearchBoundingBoxPlacesRequest } from '../map/dto/map.dto.request'

export const queryKeys = {
  searchPlaces: (searchText: string, limit: number) =>
    ['places', 'search', searchText.trim().toLowerCase(), limit] as const,
  route: (origin: LatLngTuple | null, destination: LatLngTuple | null) =>
    ['places', 'route', origin?.[0], origin?.[1], destination?.[0], destination?.[1]] as const,
  boundingBox: (request: SearchBoundingBoxPlacesRequest | null) =>
    ['places', 'bounding-box', request?.minLatitude, request?.maxLatitude, request?.minLongitude, request?.maxLongitude, request?.limit ?? 50] as const,
}
