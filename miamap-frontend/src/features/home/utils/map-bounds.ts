import type { LatLngBounds } from 'leaflet'
import type { SearchBoundingBoxPlacesRequest } from '../../../services/map/dto/map.dto.request'

export function getBoundingBoxRequest(bounds: LatLngBounds): SearchBoundingBoxPlacesRequest {
  const northWest = bounds.getNorthWest()
  const northEast = bounds.getNorthEast()
  const southWest = bounds.getSouthWest()
  const southEast = bounds.getSouthEast()

  const latitudes = [northWest.lat, northEast.lat, southWest.lat, southEast.lat]
  const longitudes = [northWest.lng, northEast.lng, southWest.lng, southEast.lng]

  return {
    minLatitude: Math.min(...latitudes),
    maxLatitude: Math.max(...latitudes),
    minLongitude: Math.min(...longitudes),
    maxLongitude: Math.max(...longitudes),
    limit: 50,
  }
}
