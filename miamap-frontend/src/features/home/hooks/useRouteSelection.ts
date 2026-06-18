import { useMemo, useState } from 'react'
import type { LatLngTuple } from 'leaflet'
import { defaultOrigin } from '../constants/map.constants'
import type { RouteSelectionApi } from '../types/home.types'

export function useRouteSelection(): RouteSelectionApi & { mapCenter: LatLngTuple } {
  const [originText, setOriginText] = useState('')
  const [destinationText, setDestinationText] = useState('')
  const [origin, setOrigin] = useState<LatLngTuple | null>(null)
  const [destination, setDestination] = useState<LatLngTuple | null>(null)
  const [statusMessage, setStatusMessage] = useState('')

  const mapCenter = useMemo<LatLngTuple>(() => {
    if (origin && destination) {
      return [(origin[0] + destination[0]) / 2, (origin[1] + destination[1]) / 2]
    }

    return origin ?? destination ?? defaultOrigin
  }, [origin, destination])

  return {
    originText,
    destinationText,
    origin,
    destination,
    statusMessage,
    mapCenter,
    setOriginText,
    setDestinationText,
    setOrigin,
    setDestination,
    setStatusMessage,
  }
}
