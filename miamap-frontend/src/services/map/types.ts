import type { LatLngTuple } from 'leaflet'

export type MapPoint = LatLngTuple

export type SearchNearbyPlacesRequest = {
	latitude: number
	longitude: number
	radiusInMeters?: number
	limit?: number
}

export type SearchBoundingBoxPlacesRequest = {
	minLatitude: number
	maxLatitude: number
	minLongitude: number
	maxLongitude: number
	limit?: number
}

export type BoundingBoxPlace = {
	placeId: number
	name: string
	category: string
	address: string | null
	latitude: number
	longitude: number
	rating: number
	reviewCount: number
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

