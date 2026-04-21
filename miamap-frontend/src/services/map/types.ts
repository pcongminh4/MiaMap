import type { LatLngTuple } from 'leaflet'

export type MapPoint = LatLngTuple

export type SearchNearbyPlacesRequest = {
	latitude: number
	longitude: number
	radiusInMeters?: number
	limit?: number
}

export type NearbyPlace = {
	placeId: number
	name: string
	category: string
	address: string | null
	latitude: number
	longitude: number
	rating: number
	reviewCount: number
	distanceInMeters: number
}

