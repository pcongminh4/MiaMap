using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Abstractions.Agent;
using Application.Results;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NetTopologySuite.Geometries;

namespace Infrastructure.Agent;

public sealed class AgentService : IAgentService
{
	private readonly HttpClient _httpClient;
	private readonly ApplicationDbContext _dbContext;
	private readonly string _apiKey;
	private readonly string _model;

	public AgentService(
		HttpClient httpClient,
		ApplicationDbContext dbContext,
		IConfiguration configuration)
	{
		_httpClient = httpClient;
		_dbContext = dbContext;
		_apiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
		_model = configuration["Gemini:Model"] ?? "gemini-2.5-flash";
	}

	public async Task<AgentChatResult> ChatAsync(
		string prompt,
		double userLat,
		double userLng,
		CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(_apiKey))
		{
			return new AgentChatResult(
				"Chức năng AI chưa cấu hình API Key. Vui lòng thêm Gemini:ApiKey vào appsettings.json.",
				[],
				"none");
		}

		// 1. RAG: Lấy địa điểm xung quanh vị trí người dùng (bán kính 2km)
		var searchPoint = new Point(userLng, userLat) { SRID = 4326 };
		var nearbyPlaces = await _dbContext.Places
			.AsNoTracking()
			.Where(p => p.IsActive)
			.Select(p => new
			{
				p.Id,
				p.Name,
				p.Category,
				p.Address,
				p.Rating,
				p.ReviewCount,
				Distance = p.Point.Distance(searchPoint) * 111000 // Quy đổi gần đúng độ sang mét
			})
			.Where(p => p.Distance <= 2000)
			.OrderBy(p => p.Distance)
			.Take(20)
			.ToListAsync(cancellationToken);

		// 2. RAG: Lấy thêm các địa điểm khớp từ khóa trong câu hỏi của người dùng để bổ sung ngữ cảnh
		var promptWords = prompt.Split(' ', StringSplitOptions.RemoveEmptyEntries)
			.Select(w => w.Trim())
			.Where(w => w.Length > 2)
			.ToList();

		List<int> textMatchedIds = [];
		if (promptWords.Count > 0)
		{
			var textMatchedPlaces = await _dbContext.Places
				.AsNoTracking()
				.Where(p => p.IsActive)
				.Where(p => promptWords.Any(word => EF.Functions.ILike(p.Name, "%" + word + "%") || EF.Functions.ILike(p.Category, "%" + word + "%")))
				.Select(p => p.Id)
				.Take(10)
				.ToListAsync(cancellationToken);

			textMatchedIds.AddRange(textMatchedPlaces);
		}

		var allPlaceIds = nearbyPlaces.Select(p => p.Id).Union(textMatchedIds).Distinct().ToList();

		var finalPlaces = await _dbContext.Places
			.AsNoTracking()
			.Where(p => allPlaceIds.Contains(p.Id))
			.Select(p => new
			{
				p.Id,
				p.Name,
				p.Category,
				p.Address,
				Latitude = p.Point.Y,
				Longitude = p.Point.X,
				p.Rating,
				p.ReviewCount,
				Distance = p.Point.Distance(searchPoint) * 111000
			})
			.ToListAsync(cancellationToken);

		// Lấy menu/sản phẩm của các quán trong ngữ cảnh
		var menuItems = await _dbContext.MenuItems
			.AsNoTracking()
			.Where(m => allPlaceIds.Contains(m.PlaceId))
			.ToListAsync(cancellationToken);

		// 3. Xây dựng ngữ cảnh Prompt (RAG Context)
		var contextBuilder = new StringBuilder();
		contextBuilder.AppendLine("Dưới đây là danh sách địa điểm xung quanh tọa độ của người dùng (trong Quận 1, TP. HCM) kèm menu sản phẩm của họ:");
		foreach (var p in finalPlaces)
		{
			var items = menuItems.Where(m => m.PlaceId == p.Id).Select(m => $"{m.Name} ({m.Price:N0}đ)").ToList();
			var itemsStr = items.Count > 0 ? " - Menu: " + string.Join(", ", items) : "";
			contextBuilder.AppendLine($"- ID: {p.Id} | Tên: {p.Name} | Loại: {p.Category} | Địa chỉ: {p.Address} | Đánh giá: {p.Rating}* ({p.ReviewCount} reviews) | Cách người dùng: {p.Distance:F0}m{itemsStr}");
		}

		var systemPrompt = $$"""
			Bạn là một trợ lý bản đồ thông minh cho ứng dụng MiaMap tại khu vực Quận 1, TP. Hồ Chí Minh.
			Vị trí hiện tại của người dùng là: Vĩ độ {{userLat}}, Kinh độ {{userLng}}.
			Sử dụng ngữ cảnh địa điểm dưới đây để trả lời yêu cầu của người dùng:
			
			{{contextBuilder.ToString()}}

			Yêu cầu phản hồi:
			1. Trả lời bằng tiếng Việt tự nhiên, ngắn gọn và hữu ích.
			2. Định dạng phản hồi của bạn BẮT BUỘC phải là cấu trúc JSON có các thuộc tính:
			   - "answer": Câu trả lời/giới thiệu thân thiện của bạn.
			   - "recommendedPlaces": Mảng chứa ID (kiểu số nguyên) của các địa điểm bạn gợi ý cho người dùng. Nếu người dùng muốn chỉ đường ("draw_route"), mảng này phải chứa chính xác 1 ID của địa điểm đích ở phần tử đầu tiên.
			   - "mapAction": Chọn một trong các hành động: "zoom_to_places" (nếu giới thiệu các quán), "draw_route" (nếu người dùng yêu cầu chỉ đường/dẫn đường đến một quán cụ thể), hoặc "none" (nếu chỉ trò chuyện thông thường).
			
			Mẫu JSON trả về:
			{
			  "answer": "Tôi tìm thấy 2 quán cafe có bán Matcha Latte gần bạn là Cộng Cà Phê và Phúc Long...",
			  "recommendedPlaces": [12, 15],
			  "mapAction": "zoom_to_places"
			}
			""";

		try
		{
			var requestBody = new
			{
				contents = new[]
				{
					new
					{
						parts = new[]
						{
							new { text = systemPrompt },
							new { text = $"Yêu cầu của người dùng: \"{prompt}\"" }
						}
					}
				},
				generationConfig = new
				{
					responseMimeType = "application/json"
				}
			};

			var jsonRequest = JsonSerializer.Serialize(requestBody);
			var httpResponse = await _httpClient.PostAsync(
				$"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}",
				new StringContent(jsonRequest, Encoding.UTF8, "application/json"),
				cancellationToken);

			if (!httpResponse.IsSuccessStatusCode)
			{
				var errorContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
				return new AgentChatResult(
					$"Lỗi kết nối Gemini API (HTTP {httpResponse.StatusCode}): {errorContent}",
					[],
					"none");
			}

			var jsonResponse = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
			using var doc = JsonDocument.Parse(jsonResponse);
			var text = doc.RootElement
				.GetProperty("candidates")[0]
				.GetProperty("content")
				.GetProperty("parts")[0]
				.GetProperty("text")
				.GetString();

			if (string.IsNullOrWhiteSpace(text))
			{
				return new AgentChatResult("Không nhận được phản hồi từ AI.", [], "none");
			}

			// Parse kết quả JSON trả về từ LLM
			var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
			var rawResult = JsonSerializer.Deserialize<GeminiChatJsonResult>(text, options);

			if (rawResult is null)
			{
				return new AgentChatResult(text, [], "none");
			}

			var recommendedPlaceDetails = finalPlaces
				.Where(p => rawResult.RecommendedPlaces != null && rawResult.RecommendedPlaces.Contains(p.Id))
				.Select(p => new
				{
					placeId = p.Id,
					name = p.Name,
					category = p.Category,
					address = p.Address,
					location = new { latitude = p.Latitude, longitude = p.Longitude },
					rating = p.Rating,
					reviewCount = p.ReviewCount
				})
				.ToList();

			return new AgentChatResult(
				rawResult.Answer ?? "Tôi tìm thấy các địa điểm này.",
				rawResult.RecommendedPlaces ?? [],
				rawResult.MapAction ?? "none",
				recommendedPlaceDetails);
		}
		catch (Exception ex)
		{
			return new AgentChatResult($"Đã xảy ra lỗi hệ thống khi gọi AI: {ex.Message}", [], "none");
		}
	}

	public async Task<AgentSearchImageResult> SearchByImageAsync(
		Stream imageStream,
		double userLat,
		double userLng,
		CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(_apiKey))
		{
			return new AgentSearchImageResult(
				false,
				"Chức năng AI chưa cấu hình API Key. Vui lòng thêm Gemini:ApiKey vào appsettings.json.",
				null, null, null, null, null);
		}

		try
		{
			// Đọc ảnh sang Base64
			using var memoryStream = new MemoryStream();
			await imageStream.CopyToAsync(memoryStream, cancellationToken);
			var imageBytes = memoryStream.ToArray();
			var base64Image = Convert.ToBase64String(imageBytes);

			var promptText = """
				Bạn là một chuyên gia định vị bản đồ tại Quận 1, TP. Hồ Chí Minh.
				Hãy nhận diện địa danh, thương hiệu cửa hàng hoặc danh lam thắng cảnh trong bức ảnh này tại Quận 1.
				Trả về kết quả dưới định dạng JSON với cấu trúc:
				{
				  "success": true hoặc false,
				  "message": "Lời giải thích ngắn gọn bằng tiếng Việt về ảnh nhận diện được",
				  "recognizedName": "Tên chi tiết địa điểm/cửa hàng/địa danh (ví dụ: 'Dinh Độc Lập', 'Bưu điện Trung tâm Sài Gòn', 'Highlands Coffee Mạc Đĩnh Chi')",
				  "brandName": "Tên thương hiệu nếu là cửa hàng chuỗi (ví dụ: 'Highlands Coffee', 'Phúc Long', 'Starbucks', 'McDonalds'), nếu là danh lam thắng cảnh độc lập thì ghi null",
				  "category": "Phân loại địa điểm (ví dụ: 'cafe', 'restaurant', 'place_of_worship', 'tourist_attraction', 'atm')"
				}
				Lưu ý: Chỉ nhận dạng các địa danh nằm trong khu vực Quận 1, TP. HCM. Nếu ảnh không rõ ràng hoặc nằm ngoài khu vực, trả về success: false.
				""";

			var requestBody = new
			{
				contents = new[]
				{
					new
					{
						parts = new object[]
						{
							new { text = promptText },
							new
							{
								inlineData = new
								{
									mimeType = "image/jpeg",
									data = base64Image
								}
							}
						}
					}
				},
				generationConfig = new
				{
					responseMimeType = "application/json"
				}
			};

			var jsonRequest = JsonSerializer.Serialize(requestBody);
			var httpResponse = await _httpClient.PostAsync(
				$"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}",
				new StringContent(jsonRequest, Encoding.UTF8, "application/json"),
				cancellationToken);

			if (!httpResponse.IsSuccessStatusCode)
			{
				var errorContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
				return new AgentSearchImageResult(
					false,
					$"Lỗi kết nối Gemini API (HTTP {httpResponse.StatusCode}): {errorContent}",
					null, null, null, null, null);
			}

			var jsonResponse = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
			using var doc = JsonDocument.Parse(jsonResponse);
			var text = doc.RootElement
				.GetProperty("candidates")[0]
				.GetProperty("content")
				.GetProperty("parts")[0]
				.GetProperty("text")
				.GetString();

			if (string.IsNullOrWhiteSpace(text))
			{
				return new AgentSearchImageResult(false, "Không thể nhận diện ảnh.", null, null, null, null, null);
			}

			var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
			var rawResult = JsonSerializer.Deserialize<GeminiImageJsonResult>(text, options);

			if (rawResult is null || !rawResult.Success)
			{
				return new AgentSearchImageResult(
					false,
					rawResult?.Message ?? "Không tìm thấy địa điểm phù hợp trong ảnh.",
					null, null, null, null, null);
			}

			// 4. Tìm kiếm trong Database theo kết quả AI nhận diện (Landmark/Tên quán -> Thương hiệu -> Thể loại)
			var searchPoint = new Point(userLng, userLat) { SRID = 4326 };

			// A. Ưu tiên tìm theo Tên Landmark cụ thể hoặc tên quán cụ thể
			var searchTerm = !string.IsNullOrWhiteSpace(rawResult.RecognizedName)
				? rawResult.RecognizedName
				: rawResult.BrandName;

			if (!string.IsNullOrWhiteSpace(searchTerm))
			{
				var matches = await _dbContext.Places
					.AsNoTracking()
					.Where(p => p.IsActive)
					.Where(p => EF.Functions.ILike(p.Name, "%" + searchTerm.Trim() + "%"))
					.Select(p => new
					{
						p.Id,
						p.Name,
						Latitude = p.Point.Y,
						Longitude = p.Point.X,
						Distance = p.Point.Distance(searchPoint) * 111000
					})
					.OrderBy(p => p.Distance)
					.Take(1)
					.ToListAsync(cancellationToken);

				if (matches.Count > 0)
				{
					return new AgentSearchImageResult(
						true,
						$"Đã nhận diện: {rawResult.Message}",
						matches[0].Id,
						matches[0].Name,
						matches[0].Latitude,
						matches[0].Longitude,
						rawResult.Category);
				}
			}

			// B. Fallback: Nếu không tìm thấy tên cụ thể trong database, tìm địa điểm thuộc Thể loại (Category) đó gần nhất
			if (!string.IsNullOrWhiteSpace(rawResult.Category))
			{
				var categoryMatches = await _dbContext.Places
					.AsNoTracking()
					.Where(p => p.IsActive)
					.Where(p => EF.Functions.ILike(p.Category, "%" + rawResult.Category.Trim() + "%") || EF.Functions.ILike(p.Name, "%" + rawResult.Category.Trim() + "%"))
					.Select(p => new
					{
						p.Id,
						p.Name,
						Latitude = p.Point.Y,
						Longitude = p.Point.X,
						Distance = p.Point.Distance(searchPoint) * 111000
					})
					.OrderBy(p => p.Distance)
					.Take(1)
					.ToListAsync(cancellationToken);

				if (categoryMatches.Count > 0)
				{
					return new AgentSearchImageResult(
						true,
						$"Đã nhận diện ảnh thuộc loại hình '{rawResult.Category}' gần bạn nhất: {categoryMatches[0].Name}. {rawResult.Message}",
						categoryMatches[0].Id,
						categoryMatches[0].Name,
						categoryMatches[0].Latitude,
						categoryMatches[0].Longitude,
						rawResult.Category);
				}
			}

			return new AgentSearchImageResult(
				true,
				$"Nhận diện được địa danh '{rawResult.RecognizedName}' nhưng địa danh này chưa có thông tin vị trí trong cơ sở dữ liệu MiaMap Quận 1.",
				null,
				rawResult.RecognizedName,
				null, null,
				rawResult.Category);
		}
		catch (Exception ex)
		{
			return new AgentSearchImageResult(
				false,
				$"Lỗi hệ thống khi nhận diện ảnh: {ex.Message}",
				null, null, null, null, null);
		}
	}

	private sealed class GeminiChatJsonResult
	{
		[JsonPropertyName("answer")]
		public string? Answer { get; set; }

		[JsonPropertyName("recommendedPlaces")]
		public List<int>? RecommendedPlaces { get; set; }

		[JsonPropertyName("mapAction")]
		public string? MapAction { get; set; }
	}

	private sealed class GeminiImageJsonResult
	{
		[JsonPropertyName("success")]
		public bool Success { get; set; }

		[JsonPropertyName("message")]
		public string? Message { get; set; }

		[JsonPropertyName("recognizedName")]
		public string? RecognizedName { get; set; }

		[JsonPropertyName("brandName")]
		public string? BrandName { get; set; }

		[JsonPropertyName("category")]
		public string? Category { get; set; }
	}
}
