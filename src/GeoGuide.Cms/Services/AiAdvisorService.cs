using System.Text;
using System.Text.Json;
using GeoGuide.Cms.Models.Api.V1;
using Microsoft.Extensions.Caching.Memory;

namespace GeoGuide.Cms.Services;

public class AiAdvisorService : IAiAdvisorService
{
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private const int MaxRequestsPerDay = 10;

    public AiAdvisorService(IMemoryCache cache, IConfiguration configuration, HttpClient httpClient)
    {
        _cache = cache;
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<EnhanceDescriptionResponse> EnhanceDescriptionAsync(EnhanceDescriptionRequest request, string userId)
    {
        var cacheKey = $"ai_advisor_usage_{userId}_{DateTime.UtcNow:yyyyMMdd}";
        var currentUsage = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
            return 0;
        });

        if (currentUsage >= MaxRequestsPerDay)
        {
            throw new InvalidOperationException("Daily request limit exceeded (10 requests/day).");
        }

        _cache.Set(cacheKey, currentUsage + 1, TimeSpan.FromDays(1));

        var apiKey = _configuration["Gemini:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new EnhanceDescriptionResponse
            {
                EnhancedDescription = GetLocalEnhancement(request),
                Provider = "local-demo"
            };
        }

        return await CallGeminiAsync(request, apiKey);
    }

    private string GetLocalEnhancement(EnhanceDescriptionRequest request)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Khám phá {request.Name} - một địa điểm tuyệt vời thuộc danh mục {request.Category}.");
        builder.AppendLine();
        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            builder.AppendLine($"Chi tiết: {request.Description}");
        }
        builder.AppendLine();
        builder.AppendLine($"📍 Địa chỉ: {request.Address}");
        if (!string.IsNullOrWhiteSpace(request.PriceRange))
        {
            builder.AppendLine($"💰 Mức giá: {request.PriceRange}");
        }
        builder.AppendLine();
        builder.AppendLine("Món ăn ở đây mang đậm hương vị truyền thống, được chế biến tỉ mỉ từ những nguyên liệu tươi ngon nhất. Không gian ấm cúng và sự phục vụ nhiệt tình chắc chắn sẽ mang lại cho bạn một trải nghiệm ẩm thực khó quên.");
        return builder.ToString();
    }

    private async Task<EnhanceDescriptionResponse> CallGeminiAsync(EnhanceDescriptionRequest request, string apiKey)
    {
        var prompt = $"Hãy cải thiện đoạn mô tả sau đây bằng tiếng Việt, làm cho nó trở nên hấp dẫn hơn đối với khách du lịch ẩm thực (food tourism). Nếu có thể, hãy nhấn mạnh hương vị, trải nghiệm và không gian.\n\n" +
                     $"Tên: {request.Name}\n" +
                     $"Mô tả gốc: {request.Description}\n" +
                     $"Danh mục: {request.Category}\n" +
                     $"Địa chỉ: {request.Address}\n" +
                     $"Mức giá: {request.PriceRange}";

        var payload = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";
        
        try
        {
            var response = await _httpClient.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
            {
                return new EnhanceDescriptionResponse
                {
                    EnhancedDescription = GetLocalEnhancement(request),
                    Provider = "local-demo"
                };
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseBody);
            
            var enhancedText = jsonDoc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text").GetString();

            if (!string.IsNullOrWhiteSpace(enhancedText))
            {
                return new EnhanceDescriptionResponse
                {
                    EnhancedDescription = enhancedText.Trim(),
                    Provider = "gemini"
                };
            }
        }
        catch
        {
            // Ignore exception and fall back to local demo
        }

        return new EnhanceDescriptionResponse
        {
            EnhancedDescription = GetLocalEnhancement(request),
            Provider = "local-demo"
        };
    }
}
