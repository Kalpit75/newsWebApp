using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class AiSummaryService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey = "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";

    public AiSummaryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetSummaryAsync(string text)
    {
        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = new[] {
                new { role = "system", content = "Summarize the following news in 1-2 sentences." },
                new { role = "user", content = text }
            }
        };
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        var responseString = await response.Content.ReadAsStringAsync();

        // Log the response for debugging
        System.Diagnostics.Debug.WriteLine("OpenAI response: " + responseString);

        try
        {
            using var doc = JsonDocument.Parse(responseString);
            if (doc.RootElement.TryGetProperty("choices", out var choices) &&
                choices.GetArrayLength() > 0 &&
                choices[0].TryGetProperty("message", out var message) &&
                message.TryGetProperty("content", out var contentProp))
            {
                return contentProp.GetString();
            }
            return "AI summary not available.";
        }
        catch
        {
            return "AI summary not available.";
        }
    }
}