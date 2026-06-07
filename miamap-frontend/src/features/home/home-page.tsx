"use client"

import dynamic from 'next/dynamic'
import { MapControls } from './components/MapControls'
import { SearchPanel } from './components/SearchPanel'
import { TopRightActions } from './components/TopRightActions'
import { maxMapZoom } from './constants/map.constants'
import { useHomeMapViewport } from './hooks/useHomeMapViewport'
import { useRoutePlanner } from './hooks/useRoutePlanner'

const HomeMap = dynamic(() => import('./components/HomeMap').then((module) => module.HomeMap), {
  ssr: false,
})

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
    clearOrigin,
    clearDestination,
  } = useRoutePlanner()
  const { map, setMap, currentZoom, zoomIn, zoomOut, boundingBoxPlaces, nearbyError } = useHomeMapViewport({ route })

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
        onSwap={() => swapDirection(map)}
        isLoading={isLoading}
        errorMessage={errorMessage}
        originSuggestions={originSuggestions}
        destinationSuggestions={destinationSuggestions}
        onSelectPlace={(type, place) => selectPlace(type, place, map)}
        onClearSuggestions={clearSuggestions}
        onClearOrigin={clearOrigin}
        onClearDestination={clearDestination}
      />

      <TopRightActions />

      {totalDistanceMeters !== null && (
        <div className="pointer-events-none absolute top-5 right-5 z-20 rounded-lg bg-white/95 px-3 py-2 text-sm font-medium text-slate-700 shadow">
          Khoang cach: {routeFound ? `${(totalDistanceMeters / 1000).toFixed(2)} km` : 'Khong tim thay duong di'}
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
