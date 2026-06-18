import type { LatLngTuple } from 'leaflet'

import type { BoundingBoxPlaceResponse } from '../../../services/map/dto/map.dto.response'

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

export type Message = {
  id: string
  sender: 'user' | 'bot' | 'system'
  text: string
  imageUrl?: string
  places?: BoundingBoxPlaceResponse[]
}

export type UseAIAgentProps = {
  mapCenter: LatLngTuple
  onSelectPlace: (place: BoundingBoxPlaceResponse) => void
  onDrawRoute: (place: BoundingBoxPlaceResponse) => void
}

export type ChatPayload = {
  prompt: string
  latitude: number
  longitude: number
}

export type ChatResponse = {
  answer: string
  recommendedPlaces: number[]
  mapAction: string
  mapActionPayload?: BoundingBoxPlaceResponse[]
}

export type ImageSearchResponse = {
  success: boolean
  message: string
  recognizedPlaceId?: number
  recognizedName?: string
  latitude?: number
  longitude?: number
  category?: string
}
