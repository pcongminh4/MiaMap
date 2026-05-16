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

export type SearchByNameOrAddressRequest = {
	searchText: string
	limit?: number
}

export type SearchResult = {
	placeId: number
	name: string
	category: string
	address: string | null
	latitude: number
	longitude: number
	rating: number
	reviewCount: number
}

export type FindRouteRequest = {
	startLatitude: number
	startLongitude: number
	endLatitude: number
	endLongitude: number
}

