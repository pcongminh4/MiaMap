import { useCallback, useState } from 'react'
import type { LatLngTuple } from 'leaflet'
import { useDebouncedValue } from '../../../hooks/useDebouncedValue'
import { useSearchPlacesQuery } from '../queries/home.queries'
import type { BackendSearchByNameOrAddressResponse } from '../../../services/map/dto/map.dto.response'
import type { RouteSelectionApi } from './useRouteSelection'

type SearchType = 'origin' | 'destination'

type RouteSearchApi = Pick<RouteSelectionApi, 'originText' | 'destinationText' | 'origin' | 'destination' | 'setOriginText' | 'setDestinationText' | 'setOrigin' | 'setDestination' | 'setStatusMessage'>

export function useRouteSearch(selection: RouteSearchApi) {
  const [originSearchText, setOriginSearchText] = useState('')
  const [destinationSearchText, setDestinationSearchText] = useState('')

  const originSearchTerm = originSearchText.trim()
  const destinationSearchTerm = destinationSearchText.trim()

  const originDebouncedSearchText = useDebouncedValue(originSearchTerm, 300)
  const destinationDebouncedSearchText = useDebouncedValue(destinationSearchTerm, 300)

  const originSuggestionsQuery = useSearchPlacesQuery(originDebouncedSearchText)
  const destinationSuggestionsQuery = useSearchPlacesQuery(destinationDebouncedSearchText)

  const originSuggestions =
    originSearchTerm && originSearchTerm === originDebouncedSearchText ? originSuggestionsQuery.data ?? [] : []
  const destinationSuggestions =
    destinationSearchTerm && destinationSearchTerm === destinationDebouncedSearchText ? destinationSuggestionsQuery.data ?? [] : []

  const setSearchText = useCallback((type: SearchType, value: string) => {
    if (type === 'origin') {
      selection.setOriginText(value)
      setOriginSearchText(value)
    } else {
      selection.setDestinationText(value)
      setDestinationSearchText(value)
    }
    selection.setStatusMessage('')
  }, [selection])

  const handleOriginTextChange = useCallback((value: string) => {
    setSearchText('origin', value)
  }, [setSearchText])

  const handleDestinationTextChange = useCallback((value: string) => {
    setSearchText('destination', value)
  }, [setSearchText])

  const clearSearchText = useCallback((type: SearchType) => {
    if (type === 'origin') {
      setOriginSearchText('')
    } else {
      setDestinationSearchText('')
    }
  }, [])

  const selectPlace = useCallback(
    async (
      type: SearchType,
      place: BackendSearchByNameOrAddressResponse,
      map?: { flyTo: (coords: LatLngTuple, zoom: number) => void } | null,
    ) => {
      const coords: LatLngTuple = [place.location.latitude, place.location.longitude]
      selection.setStatusMessage('')

      if (type === 'origin') {
        selection.setOriginText(place.name)
        selection.setOrigin(coords)
        setOriginSearchText('')

        if (!selection.destination) {
          map?.flyTo(coords, 15)
        }
      } else {
        selection.setDestinationText(place.name)
        selection.setDestination(coords)
        setDestinationSearchText('')

        if (!selection.origin) {
          map?.flyTo(coords, 15)
        }
      }
    },
    [selection],
  )

  return {
    originSuggestions,
    destinationSuggestions,
    setOriginText: handleOriginTextChange,
    setDestinationText: handleDestinationTextChange,
    selectPlace,
    clearSearchText,
  }
}
