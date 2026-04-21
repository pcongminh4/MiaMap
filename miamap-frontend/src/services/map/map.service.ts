import { http } from '../../lib/http'
import type { BackendGeocodeResponse, BackendRouteResponse } from './map.dto'
import type { IMapService } from './contracts'
import type { MapPoint, NearbyPlace, SearchNearbyPlacesRequest } from './types'

export const mapService: IMapService = {
  async geocodeLocation(query: string): Promise<MapPoint | null> {
    if (!query.trim()) {
      return null
    }

    const response = await http.get<BackendGeocodeResponse>('/maps/geocode', {
      params: {
        q: query,
      },
    })

    const location = response.data
    if (!location) {
      return null
    }

    return [location.lat, location.lng]
  },

  async getDrivingRoute(origin: MapPoint, destination: MapPoint):Promise<MapPoint[]> {
    const response = await http.post<BackendRouteResponse>('/maps/route', {
      origin: {
        lat: origin[0],
        lng: origin[1],
      },
      destination: {
        lat: destination[0],
        lng: destination[1],
      },
    })

    if (!response.data?.points?.length) {
      return [origin, destination]
    }

    return response.data.points.map((point) => [point.lat, point.lng])
  },

  async searchNearbyPlaces(request: SearchNearbyPlacesRequest) {
    const response = await http.get<NearbyPlace[]>('/places/nearby', {
      params: {
        Latitude: request.latitude,
        Longitude: request.longitude,
        RadiusInMeters: request.radiusInMeters,
        Limit: request.limit,
      },
    })

    return response.data ?? []
  },
}
