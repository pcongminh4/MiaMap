import { useMemo, useState } from 'react'
import type { LatLngTuple } from 'leaflet'
import { defaultOrigin } from '../constants/map.constants'

export type RouteSelectionApi = {
  originText: string
  destinationText: string
  origin: LatLngTuple | null
  destination: LatLngTuple | null
  statusMessage: string
  setOriginText: (value: string) => void
  setDestinationText: (value: string) => void
  setOrigin: (coords: LatLngTuple | null) => void
  setDestination: (coords: LatLngTuple | null) => void
  setStatusMessage: (message: string) => void
}

export function useRouteSelection() {
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
