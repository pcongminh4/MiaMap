import type { MapPoint } from "./map.dto.request"

export type BackendGeocodeResponse = {
  lat: number
  lng: number
} | null

export type BackendRouteResponse = {
	points: Array<{
		lat: number
		lng: number
	}>
}



export type BackendFindRouteResponse = {
	found: boolean
	pathPoints: Array<{
		latitude: number
		longitude: number
	}>
	totalDistanceMeters: number
}



export type BackendSearchByNameOrAddressResponse = {
	placeId: number
	name: string
	category: string
	address: string | null
	location: {
		latitude: number
		longitude: number
	}
	rating: number
	reviewCount: number
}

export type FindRouteResponse = {
	found: boolean
	pathPoints: MapPoint[]
	totalDistanceMeters: number
}

export type BoundingBoxPlaceResponse = {
	placeId: number
	name: string
	category: string
	address: string | null
	location: {
		latitude: number
		longitude: number
	}
	rating: number
	reviewCount: number
}

export type NearbyPlaceResponse= {
	placeId: number
	name: string
	category: string
	address: string | null
	location: {
		latitude: number
		longitude: number
	}
	rating: number
	reviewCount: number
	distanceInMeters: number
}

