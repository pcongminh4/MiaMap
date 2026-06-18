import type { LatLngTuple } from 'leaflet'

export type SearchType = 'origin' | 'destination'

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
