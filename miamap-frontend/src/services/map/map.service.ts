import { http } from '../api/http'
import type {
	BackendFindRouteResponse,
	BackendGeocodeResponse,
	BackendRouteResponse,
	BackendSearchByNameOrAddressResponse,
  BoundingBoxPlaceResponse,
  NearbyPlaceResponse,
} from './dto/map.dto.response'
import type { IMapService } from './contracts'
import type {
	FindRouteRequest,
	MapPoint,
	SearchBoundingBoxPlacesRequest,
	SearchByNameOrAddressRequest,
	SearchNearbyPlacesRequest,
} from './dto/map.dto.request'

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
    const response = await http.get<NearbyPlaceResponse[]>('/places/nearby', {
      params: {
        Latitude: request.latitude,
        Longitude: request.longitude,
        RadiusInMeters: request.radiusInMeters,
        Limit: request.limit,
      },
    })

    return response.data ?? []
  },

  async boundingBoxSearch(request: SearchBoundingBoxPlacesRequest) {
    const response = await http.get<BoundingBoxPlaceResponse[]>('/places/bounding-box', {
      params: {
        MinLatitude: request.minLatitude,
        MaxLatitude: request.maxLatitude,
        MinLongitude: request.minLongitude,
        MaxLongitude: request.maxLongitude,
        Limit: request.limit ?? 50,
      },
    })

    return response.data ?? []
  },

  async findRoute(request: FindRouteRequest) {
    const response = await http.get<BackendFindRouteResponse>('/places/route', {
      params: {
        StartLatitude: request.startLatitude,
        StartLongitude: request.startLongitude,
        EndLatitude: request.endLatitude,
        EndLongitude: request.endLongitude,
      },
    })

    const data = response.data
    return {
      found: data.found,
      pathPoints: data.pathPoints.map((point) => [point.latitude, point.longitude] as MapPoint),
      totalDistanceMeters: data.totalDistanceMeters,
    }
  },

  async searchByNameOrAddress(request: SearchByNameOrAddressRequest) {
    const response = await http.get<BackendSearchByNameOrAddressResponse[]>('/places/search', {
      params: {
        SearchText: request.searchText,
        Limit: request.limit ?? 6,
      },
    })

    return response.data ?? [];
  }
}
