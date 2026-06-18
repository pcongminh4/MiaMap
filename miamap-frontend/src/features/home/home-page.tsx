"use client"

import dynamic from 'next/dynamic'
import { useMemo, useState } from 'react'
import type { BoundingBoxPlaceResponse, BackendSearchByNameOrAddressResponse } from '../../services/map/dto/map.dto.response'
import { MapControls } from './components/MapControls'
import { PlaceDetailSheet } from './components/PlaceDetailSheet'
import { SearchPanel } from './components/SearchPanel'
import { TopRightActions } from './components/TopRightActions'
import { maxMapZoom } from './constants/map.constants'
import { useMapViewport } from './hooks/useMapViewport'
import { useRoutePlanner } from './hooks/useRoutePlanner'

const HomeMap = dynamic(() => import('./components/HomeMap').then((module) => module.HomeMap), {
  ssr: false,
})

export function HomePage() {
  const [selectedPlaceId, setSelectedPlaceId] = useState<number | null>(null)
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
  const { map, setMap, currentZoom, zoomIn, zoomOut, boundingBoxPlaces, nearbyError } = useMapViewport({ route })
  const selectedPlace = useMemo(
    () => boundingBoxPlaces.find((place) => place.placeId === selectedPlaceId) ?? null,
    [boundingBoxPlaces, selectedPlaceId],
  )

  const handleSelectMapPlace = (place: BoundingBoxPlaceResponse) => {
    setSelectedPlaceId(place.placeId)
    map?.flyTo([place.location.latitude, place.location.longitude], Math.max(map.getZoom(), 15))
  }

  const handleRouteToPlace = (place: BoundingBoxPlaceResponse) => {
    const routeTarget: BackendSearchByNameOrAddressResponse = {
      placeId: place.placeId,
      name: place.name,
      category: place.category,
      address: place.address,
      location: place.location,
      rating: place.rating,
      reviewCount: place.reviewCount,
    }

    setSelectedPlaceId(null)
    void selectPlace('destination', routeTarget, map)
  }

  return (
    <div className="relative h-screen w-full overflow-hidden bg-[#e8efe8] text-slate-900">
      <HomeMap
        mapCenter={mapCenter}
        origin={origin}
        destination={destination}
        route={route}
        boundingBoxPlaces={boundingBoxPlaces}
        selectedPlaceId={selectedPlaceId}
        maxMapZoom={maxMapZoom}
        onMapReady={setMap}
        onSelectPlace={handleSelectMapPlace}
        onMapBackgroundClick={() => setSelectedPlaceId(null)}
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

      <PlaceDetailSheet
        place={selectedPlace}
        onClose={() => setSelectedPlaceId(null)}
        onRoute={handleRouteToPlace}
      />

      {totalDistanceMeters !== null && (
        <div className="pointer-events-none absolute top-5 right-5 z-20 rounded-lg bg-white/95 px-3 py-2 text-sm font-medium text-slate-700 shadow">
          Khoảng cách: {routeFound ? `${(totalDistanceMeters / 1000).toFixed(2)} km` : 'Không tìm thấy đường đi'}
        </div>
      )}

      <div className="pointer-events-none absolute bottom-5 left-5 z-20 rounded-lg bg-white/95 px-3 py-2 text-xs font-medium text-slate-700 shadow">
        Địa điểm trong vùng: {boundingBoxPlaces.length}
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
