import { useEffect } from 'react'
import { Icon } from 'leaflet'
import type { LatLngTuple, Map as LeafletMap } from 'leaflet'
import { CircleMarker, MapContainer, Marker, Polyline, TileLayer, Tooltip, useMap } from 'react-leaflet'
import type { BoundingBoxPlaceResponse } from '../../../services/map/dto/map.dto.response'


type MapBridgeProps = {
  onMapReady: (map: LeafletMap) => void
}

type HomeMapProps = {
  mapCenter: LatLngTuple
  origin: LatLngTuple
  destination: LatLngTuple
  route: LatLngTuple[]
  boundingBoxPlaces: BoundingBoxPlaceResponse[]
  maxMapZoom: number
  onMapReady: (map: LeafletMap) => void
}

const categoryIconPathMap: Record<string, string> = {
  artwork: '/map_icons/artwork.png',
  atm: '/map_icons/atm.png',
  bakery: '/map_icons/bakery.png',
  bank: '/map_icons/bank.png',
  bar: '/map_icons/bar.png',
  cafe: '/map_icons/cafe.png',
  clothes: '/map_icons/clothes.png',
  college: '/map_icons/college.png',
  convenience: '/map_icons/convenience.png',
  dentist: '/map_icons/dentist.png',
  electronics: '/map_icons/electronics.png',
  fast_food: '/map_icons/fast_food.png',
  fuel: '/map_icons/fuel.png',
  hairdresser: '/map_icons/hairdresser.png',
  hotel: '/map_icons/hotel.png',
  ice_cream: '/map_icons/ice_cream.png',
  jewelry: '/map_icons/jewelry.png',
  laundry: '/map_icons/laundry.png',
  massage: '/map_icons/massage.png',
  place_of_worship: '/map_icons/place_of_worship.png',
  post_office: '/map_icons/post_office.png',
  pub: '/map_icons/pub.png',
  restaurant: '/map_icons/restaurant.png',
  shelter: '/map_icons/shelter.png',
  sports: '/map_icons/sports.png',
  supermarket: '/map_icons/supermarket.png',
  townhall: '/map_icons/townhall.png',
  question_mark: '/map_icons/question_mark.png',
}

const defaultPlaceIconPath = '/map_icons/question_mark.png'

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

export function HomeMap({ mapCenter, origin, destination, route, boundingBoxPlaces, maxMapZoom, onMapReady }: HomeMapProps) {
  return (
    <MapContainer center={mapCenter} zoom={13} minZoom={4} maxZoom={maxMapZoom} className="absolute inset-0 z-0">
      <MapBridge onMapReady={onMapReady} />
      <TileLayer
        attribution='&copy; OpenStreetMap contributors &copy; CARTO'
        maxZoom={maxMapZoom}
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

      {boundingBoxPlaces.map((place) => {
        const lat = place.location?.latitude
        const lng = place.location?.longitude
        if (typeof lat !== 'number' || typeof lng !== 'number' || Number.isNaN(lat) || Number.isNaN(lng)) {
          return null
        }
        return (
          <Marker
            key={place.placeId}
            position={[lat, lng]}
            icon={getPlaceIcon(place.category)}
          >
            <Tooltip direction="top" offset={[0, -8]}>
              <div className="text-xs font-semibold text-slate-800">{place.name}</div>
              <div className="text-[11px] text-slate-600">{place.category}</div>
            </Tooltip>
          </Marker>
        )
      })}

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
