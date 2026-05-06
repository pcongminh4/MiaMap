import type { BoundingBoxPlace, MapPoint, NearbyPlace, SearchBoundingBoxPlacesRequest, SearchNearbyPlacesRequest } from './types'

export interface IMapService {
  // Dùng để đổi một chuỗi địa chỉ hoặc tên địa điểm thành tọa độ bản đồ.
  geocodeLocation(query: string): Promise<MapPoint | null>

  // Dùng để lấy tuyến đường lái xe giữa hai điểm trên bản đồ.
  getDrivingRoute(origin: MapPoint, destination: MapPoint): Promise<MapPoint[]>
  
  // Dùng để tìm kiếm các địa điểm gần một vị trí cụ thể dựa trên các tiêu chí như bán kính và giới hạn số lượng kết quả.
  searchNearbyPlaces(request: SearchNearbyPlacesRequest): Promise<NearbyPlace[]>

  boundingBoxSearch(request: SearchBoundingBoxPlacesRequest): Promise<BoundingBoxPlace[]>
}
