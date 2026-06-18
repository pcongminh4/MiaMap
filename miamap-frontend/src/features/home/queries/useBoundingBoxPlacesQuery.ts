import { useQuery } from '@tanstack/react-query'
import type { BoundingBoxPlaceResponse } from '../../../services/map/dto/map.dto.response'
import type { SearchBoundingBoxPlacesRequest } from '../../../services/map/dto/map.dto.request'
import { mapService } from '../../../services/map'
import { queryKeys } from '../../../services/queries/query-keys'

export function useBoundingBoxPlacesQuery(request: SearchBoundingBoxPlacesRequest | null) {
  return useQuery<BoundingBoxPlaceResponse[]>({
    queryKey: queryKeys.boundingBox(request),
    queryFn: () => {
      if (!request) {
        throw new Error('Bounding box inputs are missing.')
      }

      return mapService.boundingBoxSearch(request)
    },
    enabled: Boolean(request),
    staleTime: 15_000,
  })
}
