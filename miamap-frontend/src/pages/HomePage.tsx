import { useEffect, useRef, useState } from 'react'
import type { Map as LeafletMap } from 'leaflet'
import { HomeMap } from '../features/home/components/HomeMap'
import { MapControls } from '../features/home/components/MapControls'
import { SearchPanel } from '../features/home/components/SearchPanel'
import { TopRightActions } from '../features/home/components/TopRightActions'
import { maxMapZoom } from '../features/home/constants/map.constants'
import { useRoutePlanner } from '../features/home/hooks/useRoutePlanner'
import type { BoundingBoxPlaceResponse } from '../services/map/dto/map.dto.response'
import { mapService } from '../services/map'


export function HomePage() {
  const {
    mapCenter,
    origin,
    destination,
    route,
    originText,
    destinationText,
    originSuggestions,
    destinationSuggestions,
    isLoading,
    errorMessage,
    totalDistanceMeters,
    routeFound,
    setOriginText,
    setDestinationText,
    selectPlace,
    swapDirection,
    locateMe,
    clearSuggestions,
  } = useRoutePlanner()
  const [map, setMap] = useState<LeafletMap | null>(null)
  const [currentZoom, setCurrentZoom] = useState(13)
  const [boundingBoxPlaces, setBoundingBoxPlaces] = useState<BoundingBoxPlaceResponse[]>([])
  const [nearbyError, setNearbyError] = useState('')
  const boundingBoxDebounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const zoomIn = () => {
    if (!map || currentZoom >= maxMapZoom) {
      return
    }

    map?.zoomIn()
  }

  const zoomOut = () => {
    map?.zoomOut()
  }

  useEffect(() => {
    if (!map) {
      return
    }

    const syncZoom = () => {
      const zoom = map.getZoom()
      console.log('Current zoom:', zoom)
      setCurrentZoom(zoom)
    }

    syncZoom()
    map.on('zoomend', syncZoom)

    return () => {
      map.off('zoomend', syncZoom)
    }
  }, [map])

  useEffect(() => {
    if (!map || route.length < 2) {
      return
    }

    map.fitBounds(route, {
      padding: [80, 80],
    })
  }, [map, route])

  useEffect(() => {
    if (!map) {
      return
    }

    const fetchBoundingBoxPlaces = async () => {
      const bounds = map.getBounds()
      const northWest = bounds.getNorthWest()
      const northEast = bounds.getNorthEast()
      const southWest = bounds.getSouthWest()
      const southEast = bounds.getSouthEast()

      const latitudes = [northWest.lat, northEast.lat, southWest.lat, southEast.lat]
      const longitudes = [northWest.lng, northEast.lng, southWest.lng, southEast.lng]

      setNearbyError('')

      try {
        const places = await mapService.boundingBoxSearch({
          minLatitude: Math.min(...latitudes),
          maxLatitude: Math.max(...latitudes),
          minLongitude: Math.min(...longitudes),
          maxLongitude: Math.max(...longitudes),
          limit: 200,
        })
        setBoundingBoxPlaces(places)
      } catch {
        setNearbyError('Khong tai duoc dia diem trong vung.')
      }
    }

    const scheduleBoundingBoxFetch = () => {
      if (boundingBoxDebounceRef.current) {
        clearTimeout(boundingBoxDebounceRef.current)
      }

      boundingBoxDebounceRef.current = setTimeout(() => {
        void fetchBoundingBoxPlaces()
      }, 300)
    }

    scheduleBoundingBoxFetch()
    map.on('move', scheduleBoundingBoxFetch)
    map.on('zoom', scheduleBoundingBoxFetch)

    return () => {
      map.off('move', scheduleBoundingBoxFetch)
      map.off('zoom', scheduleBoundingBoxFetch)

      if (boundingBoxDebounceRef.current) {
        clearTimeout(boundingBoxDebounceRef.current)
        boundingBoxDebounceRef.current = null
      }
    }
  }, [map])


  return (
    <div className="relative h-screen w-full overflow-hidden bg-[#e8efe8] text-slate-900">
      <HomeMap
        mapCenter={mapCenter}
        origin={origin}
        destination={destination}
        route={route}
        boundingBoxPlaces={boundingBoxPlaces}
        maxMapZoom={maxMapZoom}
        onMapReady={setMap}
      />

      <SearchPanel
        originText={originText}
        destinationText={destinationText}
        onOriginTextChange={setOriginText}
        onDestinationTextChange={setDestinationText}
        onSwap={swapDirection}
        isLoading={isLoading}
        errorMessage={errorMessage}
        originSuggestions={originSuggestions}
        destinationSuggestions={destinationSuggestions}
        onSelectPlace={selectPlace}
        onClearSuggestions={clearSuggestions}
      />

      <TopRightActions />

      {totalDistanceMeters !== null && (
        <div className="pointer-events-none absolute top-5 right-5 z-20 rounded-lg bg-white/95 px-3 py-2 text-sm font-medium text-slate-700 shadow">
          Khoảng cách: {routeFound ? (totalDistanceMeters / 1000).toFixed(2) + ' km' : 'Không tìm thấy đường đi'}
        </div>
      )}

      <div className="pointer-events-none absolute bottom-5 left-5 z-20 rounded-lg bg-white/95 px-3 py-2 text-xs font-medium text-slate-700 shadow">
        Bounding box places: {boundingBoxPlaces.length}
        {nearbyError ? ` - ${nearbyError}` : ''}
      </div>

      <MapControls
        onLocateMe={() => locateMe(map)}
        onZoomIn={zoomIn}
        onZoomOut={zoomOut}
        currentZoom={currentZoom}
        maxMapZoom={maxMapZoom}
      />
    </div>
  )
}
