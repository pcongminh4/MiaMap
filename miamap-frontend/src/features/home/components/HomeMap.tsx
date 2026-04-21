import { useEffect } from 'react'
import { Icon } from 'leaflet'
import type { LatLngTuple, Map as LeafletMap } from 'leaflet'
import { CircleMarker, MapContainer, Marker, Polyline, TileLayer, Tooltip, useMap } from 'react-leaflet'
import type { NearbyPlace } from '../../../services'

type MapBridgeProps = {
  onMapReady: (map: LeafletMap) => void
}

type HomeMapProps = {
  mapCenter: LatLngTuple
  origin: LatLngTuple
  destination: LatLngTuple
  route: LatLngTuple[]
  nearbyPlaces: NearbyPlace[]
  maxMapZoom: number
  onMapReady: (map: LeafletMap) => void
}

const categoryIconPathMap: Record<string, string> = {
  cafe: '/map_icons/cafe.png',
  restaurant: '/map_icons/restaurant.png',
  fast_food: '/map_icons/fast_food.png',
  fuel: '/map_icons/fuel.png',
  atm: '/map_icons/atm.png',
  bank: '/map_icons/bank.png',
  bar: '/map_icons/bar.png',
  pub: '/map_icons/pub.png',
  post_office: '/map_icons/post_office.png',
  place_of_worship: '/map_icons/place_of_worship.png',
  college: '/map_icons/college.png',
  dentist: '/map_icons/dentist.png',
  ice_cream: '/map_icons/ice_cream.png',
  hotel: '/map_icons/hotel.png',
  sports: '/map_icons/sports.png',
  supermarket: '/map_icons/supermarket.png',
  jewelry: '/map_icons/jewelry.png',
  
}

const defaultPlaceIconPath = '/map_icons/restaurant.png'

const placeIconCache = new Map<string, Icon>()

function getPlaceIcon(category: string) {
  const normalizedCategory = (category ?? '').trim().toLowerCase()
  const iconPath = categoryIconPathMap[normalizedCategory] ?? defaultPlaceIconPath

  const cachedIcon = placeIconCache.get(iconPath)
  if (cachedIcon) {
    return cachedIcon
  }

  const icon = new Icon({
    iconUrl: iconPath,
    iconSize: [32, 32],
    iconAnchor: [16, 32],
    tooltipAnchor: [0, -30],
  })

  placeIconCache.set(iconPath, icon)
  return icon
}

function MapBridge({ onMapReady }: MapBridgeProps) {
  const map = useMap()

  useEffect(() => {
    onMapReady(map)
  }, [map, onMapReady])

  return null
}

export function HomeMap({ mapCenter, origin, destination, route, nearbyPlaces, maxMapZoom, onMapReady }: HomeMapProps) {
  return (
    <MapContainer center={mapCenter} zoom={13} minZoom={4} maxZoom={maxMapZoom} className="absolute inset-0 z-0">
      <MapBridge onMapReady={onMapReady} />
      <TileLayer
        attribution='&copy; OpenStreetMap contributors &copy; CARTO'
        maxNativeZoom={maxMapZoom}
        url="https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png"
      />

      {route.length > 1 && (
        <Polyline
          positions={route}
          pathOptions={{
            color: '#1d9bf0',
            weight: 6,
            opacity: 0.9,
            lineCap: 'round',
            lineJoin: 'round',
          }}
        />
      )}

      {nearbyPlaces.map((place) => (
        <Marker
          key={place.placeId}
          position={[place.latitude, place.longitude]}
          icon={getPlaceIcon(place.category)}
        >
          <Tooltip direction="top" offset={[0, -8]}>
            <div className="text-xs font-semibold text-slate-800">{place.name}</div>
            <div className="text-[11px] text-slate-600">{place.category} - {Math.round(place.distanceInMeters)}m</div>
          </Tooltip>
        </Marker>
      ))}

      <CircleMarker center={origin} radius={9} pathOptions={{ color: '#0ea5e9', fillColor: '#38bdf8', fillOpacity: 1, weight: 3 }}>
        <Tooltip direction="top" offset={[0, -8]} permanent>
          Điểm xuất phát
        </Tooltip>
      </CircleMarker>

      <CircleMarker center={destination} radius={9} pathOptions={{ color: '#ef4444', fillColor: '#f87171', fillOpacity: 1, weight: 3 }}>
        <Tooltip direction="top" offset={[0, -8]} permanent>
          Điểm đến
        </Tooltip>
      </CircleMarker>
    </MapContainer>
  )
}
