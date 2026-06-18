import { useEffect, useRef, useState } from 'react'
import type { LatLngTuple, Map as LeafletMap } from 'leaflet'
import { useBoundingBoxPlacesQuery } from '../queries/useBoundingBoxPlacesQuery'
import { maxMapZoom } from '../constants/map.constants'
import { getBoundingBoxRequest } from '../utils/map-bounds'
import type { SearchBoundingBoxPlacesRequest } from '../../../services/map/dto/map.dto.request'

type UseMapViewportParams = {
  route: LatLngTuple[] | null
}

export function useMapViewport({ route }: UseMapViewportParams) {
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
      setBoundingBoxBounds(getBoundingBoxRequest(map.getBounds()))
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
