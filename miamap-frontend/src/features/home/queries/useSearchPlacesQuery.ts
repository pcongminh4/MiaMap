import { useQuery } from '@tanstack/react-query'
import { mapService } from '../../../services/map'
import type { BackendSearchByNameOrAddressResponse } from '../../../services/map/dto/map.dto.response'
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
