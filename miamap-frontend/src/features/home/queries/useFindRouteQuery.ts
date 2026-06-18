import { useQuery } from '@tanstack/react-query'
import type { LatLngTuple } from 'leaflet'
import { mapService } from '../../../services/map'
import { queryKeys } from '../../../services/queries/query-keys'

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
