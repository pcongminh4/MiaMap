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

