import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import type { LatLngTuple, Map as LeafletMap } from 'leaflet'
import { defaultDestination, defaultOrigin } from '../constants/map.constants'
import { mapService } from '../../../services/map'
import type { BackendSearchByNameOrAddressResponse } from '../../../services/map/dto/map.dto.response'

type SearchType = 'origin' | 'destination'

export function useRoutePlanner() {
  const [originText, setOriginText] = useState('')
  const [destinationText, setDestinationText] = useState('')
  const [origin, setOrigin] = useState<LatLngTuple | null>(null)
  const [destination, setDestination] = useState<LatLngTuple | null>(null)
  const [route, setRoute] = useState<LatLngTuple[] | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')
  const [totalDistanceMeters, setTotalDistanceMeters] = useState<number | null>(null)
  const [routeFound, setRouteFound] = useState(false)

  const [originSuggestions, setOriginSuggestions] = useState<BackendSearchByNameOrAddressResponse[]>([])
  const [destinationSuggestions, setDestinationSuggestions] = useState<BackendSearchByNameOrAddressResponse[]>([])

  const originDebounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  const destinationDebounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const mapCenter = useMemo<LatLngTuple>(() => {
    if (origin && destination) {
      return [(origin[0] + destination[0]) / 2, (origin[1] + destination[1]) / 2]
    }

    return origin ?? destination ?? defaultOrigin
  }, [origin, destination])

  const renderFindRoute = useCallback(async (nextOrigin: LatLngTuple, nextDestination: LatLngTuple) => {
    setIsLoading(true)
    setErrorMessage('')

    try {
      const result = await mapService.findRoute({
        startLatitude: nextOrigin[0],
        startLongitude: nextOrigin[1],
        endLatitude: nextDestination[0],
        endLongitude: nextDestination[1],
      })

      setRouteFound(result.found)
      setTotalDistanceMeters(result.totalDistanceMeters)

      if (result.found && result.pathPoints.length > 0) {
        setRoute(result.pathPoints)
      } else {
        setRoute(null)
        setErrorMessage('Khong tim thay duong di.')
      }
    } catch {
      setRoute(null)
      setRouteFound(false)
      setTotalDistanceMeters(null)
      setErrorMessage('Khong lay duoc route. Dang hien duong noi 2 diem.')
    } finally {
      setIsLoading(false)
    }
  }, [])

  const searchPlaces = useCallback(async (type: SearchType, query: string) => {
    if (!query.trim()) {
      if (type === 'origin') {
        setOriginSuggestions([])
      } else {
        setDestinationSuggestions([])
      }
      return
    }

    try {
      const results = await mapService.searchByNameOrAddress({ searchText: query, limit: 6 })
      if (type === 'origin') {
        setOriginSuggestions(results)
      } else {
        setDestinationSuggestions(results)
      }
    } catch {
      if (type === 'origin') {
        setOriginSuggestions([])
      } else {
        setDestinationSuggestions([])
      }
    }
  }, [])

  const handleOriginTextChange = useCallback(
    (value: string) => {
      setOriginText(value)
      setDestinationSuggestions([])

      if (originDebounceRef.current) {
        clearTimeout(originDebounceRef.current)
      }

      originDebounceRef.current = setTimeout(() => {
        void searchPlaces('origin', value)
      }, 300)
    },
    [searchPlaces],
  )

  const handleDestinationTextChange = useCallback(
    (value: string) => {
      setDestinationText(value)
      setOriginSuggestions([])

      if (destinationDebounceRef.current) {
        clearTimeout(destinationDebounceRef.current)
      }

      destinationDebounceRef.current = setTimeout(() => {
        void searchPlaces('destination', value)
      }, 300)
    },
    [searchPlaces],
  )

  const selectPlace = useCallback(
    async (type: SearchType, place: BackendSearchByNameOrAddressResponse) => {
      const coords: LatLngTuple = [place.location.latitude, place.location.longitude]

      if (type === 'origin') {
        setOriginText(place.name)
        setOrigin(coords)
        setOriginSuggestions([])
        if (destination) {
          await renderFindRoute(coords, destination)
        } else {
          setRoute(null)
          setTotalDistanceMeters(null)
          setRouteFound(false)
        }
      } else {
        setDestinationText(place.name)
        setDestination(coords)
        setDestinationSuggestions([])
        if (origin) {
          await renderFindRoute(origin, coords)
        } else {
          setRoute(null)
          setTotalDistanceMeters(null)
          setRouteFound(false)
        }
      }
    },
    [destination, origin, renderFindRoute],
  )

  const swapDirection = useCallback(async () => {
    if (!origin || !destination) {
      return
    }

    const nextOrigin = destination
    const nextDestination = origin

    setOrigin(nextOrigin)
    setDestination(nextDestination)
    setOriginText(destinationText)
    setDestinationText(originText)

    await renderFindRoute(nextOrigin, nextDestination)
  }, [destination, destinationText, origin, originText, renderFindRoute])

  const locateMe = useCallback(
    (map: LeafletMap | null) => {
      if (!navigator.geolocation) {
        setErrorMessage('Trinh duyet khong ho tro dinh vi.')
        return
      }

      navigator.geolocation.getCurrentPosition(
        async (position) => {
          const current: LatLngTuple = [position.coords.latitude, position.coords.longitude]
          setOrigin(current)
          map?.flyTo(current, Math.max(map.getZoom(), 14))
          if (destination) {
            await renderFindRoute(current, destination)
          } else {
            setRoute(null)
            setTotalDistanceMeters(null)
            setRouteFound(false)
          }
        },
        () => {
          setErrorMessage('Khong lay duoc vi tri hien tai.')
        },
        {
          enableHighAccuracy: true,
        },
      )
    },
    [destination, renderFindRoute],
  )

  const clearSuggestions = useCallback((type: SearchType) => {
    if (type === 'origin') {
      setOriginSuggestions([])
    } else {
      setDestinationSuggestions([])
    }
  }, [])

  return {
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
    setOriginText: handleOriginTextChange,
    setDestinationText: handleDestinationTextChange,
    selectPlace,
    swapDirection,
    locateMe,
    clearSuggestions,
  }
}