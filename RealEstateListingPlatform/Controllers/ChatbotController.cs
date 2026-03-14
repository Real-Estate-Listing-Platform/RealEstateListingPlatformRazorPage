<<<<<<< HEAD
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
=======
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
>>>>>>> feature/razor-page

namespace RealEstateListingPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
<<<<<<< HEAD
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public ChatbotController(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public class ChatRequest
        {
            public string Message { get; set; }
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Tin nhắn không được để trống.");

            var apiKey = _configuration["Gemini:ApiKey"];


            var geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";
            var systemPrompt = "Bạn là một trợ lý ảo chuyên nghiệp tư vấn về bất động sản cho RealEstateListingPlatform. Hãy trả lời ngắn gọn, lịch sự bằng tiếng Việt.";
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {                          
                            new { text = $"{systemPrompt}\n\nNgười dùng hỏi: {request.Message}" }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    topK = 1,
                    topP = 1,
                    maxOutputTokens = 2048
                },                 
                safetySettings = new[]
                {
                    new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_NONE" }
                }
            };

            try
            {
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(geminiUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();                
                Console.WriteLine("Gemini Raw Response: " + responseString);
                using var jsonDoc = JsonDocument.Parse(responseString);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];  
                    
                    if (firstCandidate.TryGetProperty("content", out var contentNode) &&
                        contentNode.TryGetProperty("parts", out var parts) &&
                        parts.GetArrayLength() > 0)
                    {
                        var replyMessage = parts[0].GetProperty("text").GetString();
                        return Ok(new { reply = replyMessage });
                    }

                    if (firstCandidate.TryGetProperty("finishReason", out var reason))
                    {
                        string reasonStr = reason.GetString();
                        return Ok(new { reply = $"AI không thể trả lời. Lý do hệ thống: {reasonStr}. Vui lòng thử câu hỏi khác." });
                    }
                }

             
                return Ok(new { reply = "Cấu trúc phản hồi không xác định.", debug = responseString });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { reply = "Lỗi xử lý hệ thống: " + ex.Message });
            }
=======
        private readonly IChatbotService _chatbotService;
        private const string SessionKey = "ChatHistory";
        private const int MaxHistoryMessages = 50;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            var history = GetSessionHistory();
            return Ok(new { success = true, history });
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatbotRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { success = false, message = "Message cannot be empty" });

            var history = GetSessionHistory();
            var response = await _chatbotService.ChatAsync(request.Message, history);

            if (response.Success)
            {
                history.Add(new ChatMessageDto { Role = "user", Content = request.Message, Timestamp = DateTime.UtcNow });
                history.Add(new ChatMessageDto { Role = "model", Content = response.Message, Timestamp = DateTime.UtcNow });

                if (history.Count > MaxHistoryMessages)
                    history = history.Skip(history.Count - MaxHistoryMessages).ToList();

                SaveSessionHistory(history);
            }

            return Ok(response);
        }

        [HttpPost("recommendations")]
        public async Task<IActionResult> GetRecommendations([FromBody] ChatbotRequestDto request)
        {
            var listings = await _chatbotService.GetListingRecommendationsAsync(request.Message);
            return Ok(new { success = true, listings });
        }

        [HttpDelete("history")]
        public IActionResult ClearHistory()
        {
            HttpContext.Session.Remove(SessionKey);
            return Ok(new { success = true, message = "Chat history cleared" });
        }

        private List<ChatMessageDto> GetSessionHistory()
        {
            var json = HttpContext.Session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json)) return new List<ChatMessageDto>();
            return JsonSerializer.Deserialize<List<ChatMessageDto>>(json) ?? new List<ChatMessageDto>();
        }

        private void SaveSessionHistory(List<ChatMessageDto> history)
        {
            HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(history));
>>>>>>> feature/razor-page
        }
    }
}
