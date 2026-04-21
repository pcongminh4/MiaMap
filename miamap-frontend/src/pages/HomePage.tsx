import { useEffect, useState } from 'react'
import type { Map as LeafletMap } from 'leaflet'
import { HomeMap } from '../features/home/components/HomeMap'
import { MapControls } from '../features/home/components/MapControls'
import { SearchPanel } from '../features/home/components/SearchPanel'
import { TopRightActions } from '../features/home/components/TopRightActions'
import { maxMapZoom } from '../features/home/constants/map.constants'
import { useRoutePlanner } from '../features/home/hooks/useRoutePlanner'
import { mapService, type NearbyPlace } from '../services'

export function HomePage() {
  const {
    mapCenter,
    origin,
    destination,
    route,
    originText,
    destinationText,
    isLoading,
    errorMessage,
    setOriginText,
    setDestinationText,
    searchAndSetPoint,
    swapDirection,
    locateMe,
  } = useRoutePlanner()
  const [map, setMap] = useState<LeafletMap | null>(null)
  const [currentZoom, setCurrentZoom] = useState(13)
  const [nearbyPlaces, setNearbyPlaces] = useState<NearbyPlace[]>([])
  const [nearbyError, setNearbyError] = useState('')

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
      setCurrentZoom(map.getZoom())
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
    const fetchNearbyPlaces = async () => {
      setNearbyError('')

      try {
        const places = await mapService.searchNearbyPlaces({
          latitude: 10.773,
          longitude: 106.699,
          radiusInMeters: 1000,
          limit: 20,
        })

        setNearbyPlaces(places)
      } catch {
        setNearbyError('Khong tai duoc dia diem gan day.')
      }
    }

    void fetchNearbyPlaces()
  }, [])

  return (
    <div className="relative h-screen w-full overflow-hidden bg-[#e8efe8] text-slate-900">
      <HomeMap
        mapCenter={mapCenter}
        origin={origin}
        destination={destination}
        route={route}
        nearbyPlaces={nearbyPlaces}
        maxMapZoom={maxMapZoom}
        onMapReady={setMap}
      />

      <SearchPanel
        originText={originText}
        destinationText={destinationText}
        onOriginTextChange={setOriginText}
        onDestinationTextChange={setDestinationText}
        onSearch={searchAndSetPoint}
        onSwap={swapDirection}
        isLoading={isLoading}
        errorMessage={errorMessage}
      />

      <TopRightActions />

      <div className="pointer-events-none absolute bottom-5 left-5 z-20 rounded-lg bg-white/95 px-3 py-2 text-xs font-medium text-slate-700 shadow">
        Nearby places: {nearbyPlaces.length}
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
