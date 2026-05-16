
import type { BackendSearchByNameOrAddressResponse, BoundingBoxPlaceResponse, FindRouteResponse, NearbyPlaceResponse } from './dto/map.dto.response'
import type {
	FindRouteRequest,
	MapPoint,
	SearchBoundingBoxPlacesRequest,
	SearchByNameOrAddressRequest,
	SearchNearbyPlacesRequest,
} from './dto/map.dto.request'

export interface IMapService {
	// Dùng để đổi một chuỗi địa chỉ hoặc tên địa điểm thành tọa độ bản đồ.
	geocodeLocation(query: string): Promise<MapPoint | null>

	// Dùng để lấy tuyến đường lái xe giữa hai điểm trên bản đồ.
	getDrivingRoute(origin: MapPoint, destination: MapPoint): Promise<MapPoint[]>

	// Dùng để tìm kiếm các địa điểm gần một vị trí cụ thể dựa trên các tiêu chí như bán kính và giới hạn số lượng kết quả.
	searchNearbyPlaces(request: SearchNearbyPlacesRequest): Promise<NearbyPlaceResponse[]>

	boundingBoxSearch(request: SearchBoundingBoxPlacesRequest): Promise<BoundingBoxPlaceResponse[]>

	// Dùng để tìm đường đi giữa 2 điểm (sử dụng Dijkstra algorithm).
	findRoute(request: FindRouteRequest): Promise<FindRouteResponse>

	// Dùng để tìm kiếm địa điểm theo tên hoặc địa chỉ.
	searchByNameOrAddress(request: SearchByNameOrAddressRequest): Promise<BackendSearchByNameOrAddressResponse[]>
}
