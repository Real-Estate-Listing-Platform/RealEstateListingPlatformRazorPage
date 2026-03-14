using BLL.DTOs;
using DAL.Models;
using DAL.Repositories;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BLL.Services.Implementation
{
    public class ChatbotService : IChatbotService
    {
        private readonly IListingRepository _listingRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _geminiApiKey;
        private readonly string _geminiApiUrl;

        private const string SystemPrompt = @"Bạn là trợ lý AI thông minh của nền tảng bất động sản Estately. 
Nhiệm vụ của bạn là giúp người dùng tìm kiếm, tư vấn và giải đáp thắc mắc về bất động sản tại Việt Nam.

Bạn có thể giúp:
- Tư vấn tìm kiếm căn hộ, nhà, đất, biệt thự phù hợp với nhu cầu
- Giải thích về giá cả thị trường, xu hướng bất động sản
- Tư vấn về pháp lý (Sổ đỏ, Sổ hồng, Hợp đồng mua bán)
- Giới thiệu các listing phù hợp từ hệ thống

Khi người dùng muốn tìm BĐS, hãy hỏi thêm về:
- Mục đích: mua hay thuê
- Loại BĐS: căn hộ, nhà phố, biệt thự, đất, văn phòng
- Khu vực (quận/huyện/thành phố)
- Ngân sách
- Diện tích, số phòng ngủ

Trả lời ngắn gọn, thân thiện và chuyên nghiệp bằng tiếng Việt.
Khi giới thiệu listing, đừng liệt kê chi tiết mà để hệ thống tự hiển thị thẻ bất động sản.";

        public ChatbotService(
            IListingRepository listingRepository,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _listingRepository = listingRepository;
            _httpClientFactory = httpClientFactory;
            _geminiApiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
            _geminiApiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_geminiApiKey}";
        }

        public async Task<ChatbotResponseDto> ChatAsync(string userMessage, List<ChatMessageDto> history)
        {
            try
            {
                // Get recommended listings based on user message
                var suggestedListings = await GetListingRecommendationsAsync(userMessage);

                // Build context with available listings summary
                var listingContext = BuildListingContext(suggestedListings);

                // Build Gemini request
                var contents = new List<object>();

                // Add chat history
                foreach (var msg in history.TakeLast(8))
                {
                    contents.Add(new
                    {
                        role = msg.Role,
                        parts = new[] { new { text = msg.Content } }
                    });
                }

                // Add current user message with listing context
                var userMessageWithContext = string.IsNullOrEmpty(listingContext)
                    ? userMessage
                    : $"{userMessage}\n\n[Hệ thống: Có {suggestedListings.Count} BĐS phù hợp trong database: {listingContext}]";

                contents.Add(new
                {
                    role = "user",
                    parts = new[] { new { text = userMessageWithContext } }
                });

                var requestBody = new
                {
                    system_instruction = new
                    {
                        parts = new[] { new { text = SystemPrompt } }
                    },
                    contents,
                    generationConfig = new
                    {
                        temperature = 0.7,
                        maxOutputTokens = 1024,
                    }
                };

                var client = _httpClientFactory.CreateClient();
                var json = JsonSerializer.Serialize(requestBody);

                // Retry up to 3 times on 429 (rate limit) with increasing delays
                System.Net.Http.HttpResponseMessage response;
                string responseBody;
                int attempt = 0;
                int[] retryDelays = { 0, 5000, 10000 };
                do
                {
                    if (attempt > 0) await Task.Delay(retryDelays[attempt]);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    response = await client.PostAsync(_geminiApiUrl, content);
                    responseBody = await response.Content.ReadAsStringAsync();
                    attempt++;
                } while ((int)response.StatusCode == 429 && attempt < 3);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = (int)response.StatusCode == 429
                        ? "⏳ API đang bận (giới hạn tốc độ). Vui lòng chờ vài giây rồi thử lại."
                        : (int)response.StatusCode == 401 || (int)response.StatusCode == 403
                            ? "🔑 API key không hợp lệ. Vui lòng kiểm tra cấu hình Gemini:ApiKey trong appsettings.json."
                            : "Xin lỗi, tôi đang gặp sự cố kết nối. Vui lòng thử lại sau.";
                    return new ChatbotResponseDto
                    {
                        Success = false,
                        Message = errorMessage,
                        Error = responseBody
                    };
                }

                var geminiResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
                var aiText = geminiResponse
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "Xin lỗi, tôi không thể trả lời lúc này.";

                return new ChatbotResponseDto
                {
                    Success = true,
                    Message = aiText,
                    SuggestedListings = suggestedListings.Take(3).ToList()
                };
            }
            catch (Exception ex)
            {
                return new ChatbotResponseDto
                {
                    Success = false,
                    Message = "Xin lỗi, đã có lỗi xảy ra. Vui lòng thử lại.",
                    Error = ex.Message
                };
            }
        }

        public async Task<List<ListingDto>> GetListingRecommendationsAsync(string userMessage)
        {
            try
            {
                var allListings = await _listingRepository.GetPublishedListingsAsync();
                var published = allListings.ToList();

                if (!published.Any()) return new List<ListingDto>();

                var lower = userMessage.ToLower();

                // Filter by transaction type
                var filtered = published.AsEnumerable();

                if (lower.Contains("thuê") || lower.Contains("thue") || lower.Contains("rent"))
                    filtered = filtered.Where(l => l.TransactionType?.ToLower() == "rent");
                else if (lower.Contains("mua") || lower.Contains("bán") || lower.Contains("ban") || lower.Contains("buy") || lower.Contains("sell"))
                    filtered = filtered.Where(l => l.TransactionType?.ToLower() == "sell");

                // Filter by property type
                if (lower.Contains("căn hộ") || lower.Contains("can ho") || lower.Contains("apartment") || lower.Contains("chung cư"))
                    filtered = filtered.Where(l => l.PropertyType?.ToLower() == "apartment");
                else if (lower.Contains("nhà") || lower.Contains("nha") || lower.Contains("house"))
                    filtered = filtered.Where(l => l.PropertyType?.ToLower() == "house");
                else if (lower.Contains("biệt thự") || lower.Contains("biet thu") || lower.Contains("villa"))
                    filtered = filtered.Where(l => l.PropertyType?.ToLower() == "villa");
                else if (lower.Contains("đất") || lower.Contains("dat") || lower.Contains("land"))
                    filtered = filtered.Where(l => l.PropertyType?.ToLower() == "land");

                // Filter by district/city keywords
                var districts = new[] { "quận 1", "quận 2", "quận 3", "quận 4", "quận 5", "quận 6", "quận 7", "quận 8", "quận 9", "quận 10",
                    "bình thạnh", "gò vấp", "tân bình", "phú nhuận", "bình chánh", "hóc môn", "củ chi",
                    "thủ đức", "q1", "q2", "q3", "q7", "q9", "hà nội", "đà nẵng", "hải phòng" };

                foreach (var district in districts)
                {
                    if (lower.Contains(district))
                    {
                        filtered = filtered.Where(l =>
                            (l.District != null && l.District.ToLower().Contains(district)) ||
                            (l.City != null && l.City.ToLower().Contains(district)));
                        break;
                    }
                }

                // Filter by price range
                var priceMatch = Regex.Match(lower, @"dưới\s*(\d+)\s*(tỷ|triệu|ty|trieu)");
                if (priceMatch.Success && decimal.TryParse(priceMatch.Groups[1].Value, out var priceVal))
                {
                    var multiplier = priceMatch.Groups[2].Value.Contains("tỷ") || priceMatch.Groups[2].Value.Contains("ty") ? 1_000_000_000m : 1_000_000m;
                    filtered = filtered.Where(l => l.Price <= priceVal * multiplier);
                }

                // Filter by bedrooms
                var bedroomMatch = Regex.Match(lower, @"(\d+)\s*(phòng ngủ|bedroom|phong ngu)");
                if (bedroomMatch.Success && int.TryParse(bedroomMatch.Groups[1].Value, out var bedrooms))
                    filtered = filtered.Where(l => l.Bedrooms == bedrooms);

                var result = filtered.Take(5).ToList();

                // If no match, return top 3 latest listings
                if (!result.Any())
                    result = published.OrderByDescending(l => l.CreatedAt).Take(3).ToList();

                return result.Select(MapToDto).ToList();
            }
            catch
            {
                return new List<ListingDto>();
            }
        }

        private string BuildListingContext(List<ListingDto> listings)
        {
            if (!listings.Any()) return string.Empty;

            return string.Join("; ", listings.Select((ListingDto l) =>
                $"'{l.Title}' tại {l.District}, {l.City} - {FormatPrice(l.Price)} ({l.PropertyType}, {l.TransactionType})"));
        }

        private string FormatPrice(decimal price)
        {
            if (price >= 1_000_000_000)
                return $"{price / 1_000_000_000:F1} tỷ";
            if (price >= 1_000_000)
                return $"{price / 1_000_000:F0} triệu";
            return $"{price:N0} đồng";
        }

        private ListingDto MapToDto(Listing l) => new ListingDto
        {
            Id = l.Id,
            ListerId = l.ListerId,
            Title = l.Title ?? string.Empty,
            Description = l.Description,
            TransactionType = l.TransactionType,
            PropertyType = l.PropertyType,
            Price = l.Price,
            District = l.District,
            City = l.City,
            Ward = l.Ward,
            Area = l.Area,
            Bedrooms = l.Bedrooms,
            Bathrooms = l.Bathrooms,
            Status = l.Status,
            IsBoosted = l.IsBoosted,
            ListingMedia = l.ListingMedia?.Select(m => new ListingMediaDto
            {
                Id = m.Id,
                Url = m.Url ?? string.Empty,
                MediaType = m.MediaType ?? "image",
                SortOrder = m.SortOrder ?? 0
            }).ToList() ?? new()
        };
    }
}
