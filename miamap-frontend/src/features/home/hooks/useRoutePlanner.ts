import { useCallback, useEffect, useMemo, useState } from 'react'
import type { LatLngTuple, Map as LeafletMap } from 'leaflet'
import { defaultDestination, defaultOrigin } from '../constants/map.constants'
import { mapService } from '../../../services'

type SearchType = 'origin' | 'destination'

export function useRoutePlanner() {
  const [originText, setOriginText] = useState('Cho Ben Thanh, Ho Chi Minh')
  const [destinationText, setDestinationText] = useState('Landmark 81, Ho Chi Minh')
  const [origin, setOrigin] = useState<LatLngTuple>(defaultOrigin)
  const [destination, setDestination] = useState<LatLngTuple>(defaultDestination)
  const [route, setRoute] = useState<LatLngTuple[]>([defaultOrigin, defaultDestination])
  const [isLoading, setIsLoading] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')

  const mapCenter = useMemo<LatLngTuple>(() => {
    return [(origin[0] + destination[0]) / 2, (origin[1] + destination[1]) / 2]
  }, [origin, destination])

  const renderRoute = useCallback(async (nextOrigin: LatLngTuple, nextDestination: LatLngTuple) => {
    setIsLoading(true)
    setErrorMessage('')

    try {
      const points = await mapService.getDrivingRoute(nextOrigin, nextDestination)
      setRoute(points)
    } catch {
      setRoute([nextOrigin, nextDestination])
      setErrorMessage('Khong lay duoc route online. Dang hien duong noi 2 diem.')
    } finally {
      setIsLoading(false)
    }
  }, [])

  const searchAndSetPoint = useCallback(
    async (type: SearchType) => {
      const query = type === 'origin' ? originText : destinationText

      try {
        const point = await mapService.geocodeLocation(query)
        if (!point) {
          setErrorMessage('Khong tim thay dia diem. Thu doi tu khoa khac.')
          return
        }

        if (type === 'origin') {
          setOrigin(point)
          await renderRoute(point, destination)
        } else {
          setDestination(point)
          await renderRoute(origin, point)
        }
      } catch {
        setErrorMessage('Khong the tim kiem dia diem luc nay.')
      }
    },
    [destination, destinationText, origin, originText, renderRoute],
  )

  const swapDirection = useCallback(async () => {
    const nextOrigin = destination
    const nextDestination = origin

    setOrigin(nextOrigin)
    setDestination(nextDestination)
    setOriginText(destinationText)
    setDestinationText(originText)

    await renderRoute(nextOrigin, nextDestination)
  }, [destination, destinationText, origin, originText, renderRoute])

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
          await renderRoute(current, destination)
        },
        () => {
          setErrorMessage('Khong lay duoc vi tri hien tai.')
        },
        {
          enableHighAccuracy: true,
        },
      )
    },
    [destination, renderRoute],
  )

  useEffect(() => {
    void renderRoute(origin, destination)
  }, [renderRoute])

  return {
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
  }
}
