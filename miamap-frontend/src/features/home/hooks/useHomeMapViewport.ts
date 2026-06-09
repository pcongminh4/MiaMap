import { useEffect, useRef, useState } from 'react'
import type { LatLngTuple, Map as LeafletMap } from 'leaflet'
import { maxMapZoom } from '../constants/map.constants'
import { useBoundingBoxPlacesQuery } from '../queries/home.queries'
import type { SearchBoundingBoxPlacesRequest } from '../../../services/map/dto/map.dto.request'

type UseHomeMapViewportParams = {
  route: LatLngTuple[] | null
}

export function useHomeMapViewport({ route }: UseHomeMapViewportParams) {
  const [map, setMap] = useState<LeafletMap | null>(null)
  const [currentZoom, setCurrentZoom] = useState(13)
  const [boundingBoxBounds, setBoundingBoxBounds] = useState<SearchBoundingBoxPlacesRequest | null>(null)
  const boundingBoxDebounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const boundingBoxQuery = useBoundingBoxPlacesQuery(boundingBoxBounds)
  const boundingBoxPlaces = boundingBoxQuery.data ?? []
  const nearbyError = boundingBoxQuery.isError ? 'Khong tai duoc dia diem trong vung.' : ''

  const zoomIn = () => {
    if (!map || currentZoom >= maxMapZoom) {
      return
    }

    map.zoomIn()
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
    if (!map || !route || route.length < 2) {
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

    const updateBoundingBox = () => {
      const bounds = map.getBounds()
      const northWest = bounds.getNorthWest()
      const northEast = bounds.getNorthEast()
      const southWest = bounds.getSouthWest()
      const southEast = bounds.getSouthEast()

      const latitudes = [northWest.lat, northEast.lat, southWest.lat, southEast.lat]
      const longitudes = [northWest.lng, northEast.lng, southWest.lng, southEast.lng]

      setBoundingBoxBounds({
        minLatitude: Math.min(...latitudes),
        maxLatitude: Math.max(...latitudes),
        minLongitude: Math.min(...longitudes),
        maxLongitude: Math.max(...longitudes),
      })
    }

    const scheduleBoundingBoxFetch = () => {
      if (boundingBoxDebounceRef.current) {
        clearTimeout(boundingBoxDebounceRef.current)
      }

      boundingBoxDebounceRef.current = setTimeout(() => {
        updateBoundingBox()
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

  return {
    map,
    setMap,
    currentZoom,
    zoomIn,
    zoomOut,
    boundingBoxPlaces,
    nearbyError,
  }
}
