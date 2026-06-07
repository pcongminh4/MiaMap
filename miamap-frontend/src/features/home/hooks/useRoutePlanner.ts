import { useFindRouteQuery } from '../queries/home.queries'
import { useRouteActions } from './useRouteActions'
import { useRouteSearch } from './useRouteSearch'
import { useRouteSelection } from './useRouteSelection'

export function useRoutePlanner() {
  const selection = useRouteSelection()
  const search = useRouteSearch(selection)
  const actions = useRouteActions(selection, search)
  const routeQuery = useFindRouteQuery(selection.origin, selection.destination)

  const routeData = routeQuery.data
  const route = routeData?.found && routeData.pathPoints.length > 0 ? routeData.pathPoints : null
  const totalDistanceMeters = routeData?.totalDistanceMeters ?? null
  const routeFound = routeData?.found ?? false
  const isLoading = routeQuery.isFetching
  const errorMessage =
    selection.statusMessage ||
    (routeQuery.isError
      ? 'Khong lay duoc route. Dang hien duong noi 2 diem.'
      : selection.origin && selection.destination && routeData && !routeData.found
        ? 'Khong tim thay duong di.'
        : '')

  return {
    mapCenter: selection.mapCenter,
    origin: selection.origin,
    destination: selection.destination,
    route,
    originText: selection.originText,
    destinationText: selection.destinationText,
    originSuggestions: search.originSuggestions,
    destinationSuggestions: search.destinationSuggestions,
    isLoading,
    errorMessage,
    totalDistanceMeters,
    routeFound,
    setOriginText: search.setOriginText,
    setDestinationText: search.setDestinationText,
    selectPlace: search.selectPlace,
    swapDirection: actions.swapDirection,
    locateMe: actions.locateMe,
    clearSuggestions: actions.clearSuggestions,
    clearOrigin: actions.clearOrigin,
    clearDestination: actions.clearDestination,
  }
}
