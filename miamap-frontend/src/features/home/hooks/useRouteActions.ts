import { useCallback } from 'react'
import type { LatLngTuple, Map as LeafletMap } from 'leaflet'
import type { RouteSelectionApi } from '../types/home.types'

type RouteSearchActions = {
  clearSearchText: (type: 'origin' | 'destination') => void
}

type RouteActionsSelection = Pick<
  RouteSelectionApi,
  'origin' | 'destination' | 'originText' | 'destinationText' | 'setOrigin' | 'setDestination' | 'setOriginText' | 'setDestinationText' | 'setStatusMessage'
>

export function useRouteActions(selection: RouteActionsSelection, search: RouteSearchActions) {
  const swapDirection = useCallback(
    async (map?: LeafletMap | null) => {
      if (!selection.origin || !selection.destination) {
        return
      }

      const nextOrigin = selection.destination
      const nextDestination = selection.origin

      selection.setOrigin(nextOrigin)
      selection.setDestination(nextDestination)
      selection.setOriginText(selection.destinationText)
      selection.setDestinationText(selection.originText)
      selection.setStatusMessage('')
      search.clearSearchText('origin')
      search.clearSearchText('destination')

      map?.flyTo(nextOrigin, 15)
    },
    [search, selection],
  )

  const locateMe = useCallback(
    (map?: { flyTo: (coords: LatLngTuple, zoom: number) => void; getZoom: () => number } | null) => {
      if (!navigator.geolocation) {
        selection.setStatusMessage('Trinh duyet khong ho tro dinh vi.')
        return
      }

      navigator.geolocation.getCurrentPosition(
        (position) => {
          const current: LatLngTuple = [position.coords.latitude, position.coords.longitude]
          selection.setOrigin(current)
          selection.setStatusMessage('')
          search.clearSearchText('origin')
          map?.flyTo(current, Math.max(map.getZoom(), 14))
        },
        () => {
          selection.setStatusMessage('Khong lay duoc vi tri hien tai.')
        },
        {
          enableHighAccuracy: true,
        },
      )
    },
    [search, selection],
  )

  const clearOrigin = useCallback(() => {
    selection.setOriginText('')
    selection.setOrigin(null)
    selection.setStatusMessage('')
    search.clearSearchText('origin')
  }, [search, selection])

  const clearDestination = useCallback(() => {
    selection.setDestinationText('')
    selection.setDestination(null)
    selection.setStatusMessage('')
    search.clearSearchText('destination')
  }, [search, selection])

  const clearSuggestions = search.clearSearchText

  return {
    swapDirection,
    locateMe,
    clearOrigin,
    clearDestination,
    clearSuggestions,
  }
}
