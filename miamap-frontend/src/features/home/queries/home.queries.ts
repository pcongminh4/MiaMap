import { useQuery } from '@tanstack/react-query'
import type { LatLngTuple } from 'leaflet'
import { mapService } from '../../../services/map'
import type { BoundingBoxPlaceResponse, BackendSearchByNameOrAddressResponse } from '../../../services/map/dto/map.dto.response'
import type { BoundingBoxBounds } from '../../../services/queries/query-keys'
import { queryKeys } from '../../../services/queries/query-keys'

export function useSearchPlacesQuery(searchText: string, limit = 6) {
  const trimmedSearchText = searchText.trim()

  return useQuery<BackendSearchByNameOrAddressResponse[]>({
    queryKey: queryKeys.searchPlaces(trimmedSearchText, limit),
    queryFn: () => mapService.searchByNameOrAddress({ searchText: trimmedSearchText, limit }),
    enabled: trimmedSearchText.length > 0,
    staleTime: 30_000,
  })
}

export function useFindRouteQuery(origin: LatLngTuple | null, destination: LatLngTuple | null) {
  return useQuery({
    queryKey: queryKeys.route(origin, destination),
    queryFn: () => {
      if (!origin || !destination) {
        throw new Error('Route inputs are missing.')
      }

      return mapService.findRoute({
        startLatitude: origin[0],
        startLongitude: origin[1],
        endLatitude: destination[0],
        endLongitude: destination[1],
      })
    },
    enabled: Boolean(origin && destination),
  })
}

export function useBoundingBoxPlacesQuery(bounds: BoundingBoxBounds | null, limit = 200) {
  return useQuery<BoundingBoxPlaceResponse[]>({
    queryKey: queryKeys.boundingBox(bounds, limit),
    queryFn: () => {
      if (!bounds) {
        throw new Error('Bounding box inputs are missing.')
      }

      return mapService.boundingBoxSearch({
        minLatitude: bounds.minLatitude,
        maxLatitude: bounds.maxLatitude,
        minLongitude: bounds.minLongitude,
        maxLongitude: bounds.maxLongitude,
        limit,
      })
    },
    enabled: Boolean(bounds),
    staleTime: 15_000,
  })
}
