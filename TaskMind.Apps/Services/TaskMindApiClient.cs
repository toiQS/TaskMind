using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace TaskMind.Apps.Services;

public sealed class TaskMindApiClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient = new();
    private string? _accessToken;

    public async Task<T> GetAsync<T>(
        string baseUrl, string path, CancellationToken cancellationToken = default)
    {
        SetBaseAddress(baseUrl);
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (!string.IsNullOrWhiteSpace(_accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResponseAsync<T>(response, cancellationToken);
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(
        string baseUrl, string path, TRequest body, CancellationToken cancellationToken = default)
    {
        SetBaseAddress(baseUrl);
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };
        if (!string.IsNullOrWhiteSpace(_accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await ReadResponseAsync<TResponse>(response, cancellationToken);
    }

    public void SetAccessToken(string? accessToken) => _accessToken = accessToken;

    private void SetBaseAddress(string baseUrl)
    {
        if (!Uri.TryCreate(baseUrl.Trim().TrimEnd('/') + "/", UriKind.Absolute, out var address) ||
            (address.Scheme != Uri.UriSchemeHttp && address.Scheme != Uri.UriSchemeHttps))
            throw new ArgumentException("Địa chỉ API phải bắt đầu bằng http:// hoặc https://.", nameof(baseUrl));
        _httpClient.BaseAddress = address;
    }

    private static async Task<T> ReadResponseAsync<T>(
        HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var message = TryReadMessage(content) ??
                $"API trả về lỗi {(int)response.StatusCode} ({response.ReasonPhrase}).";
            throw new HttpRequestException(message, null, response.StatusCode);
        }

        try
        {
            var envelope = JsonSerializer.Deserialize<ApiEnvelope<T>>(content, JsonOptions);
            if (envelope is null)
                throw new JsonException("API trả về dữ liệu rỗng.");
            if (!string.Equals(envelope.Status, "Success", StringComparison.OrdinalIgnoreCase))
                throw new HttpRequestException(
                    string.IsNullOrWhiteSpace(envelope.Message) ? "Yêu cầu API không thành công." : envelope.Message);
            if (envelope.Data is null)
                throw new JsonException("API không trả về trường dữ liệu cần thiết.");
            return envelope.Data;
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Không đọc được phản hồi từ API TaskMind.", exception);
        }
    }

    private static string? TryReadMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;
        try
        {
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.TryGetProperty("message", out var message) &&
                message.ValueKind == JsonValueKind.String)
                return message.GetString();
            if (document.RootElement.TryGetProperty("detail", out var detail) &&
                detail.ValueKind == JsonValueKind.String)
                return detail.GetString();
            if (document.RootElement.TryGetProperty("title", out var title) &&
                title.ValueKind == JsonValueKind.String)
                return title.GetString();
        }
        catch (JsonException)
        {
            return null;
        }
        return null;
    }

    public void Dispose() => _httpClient.Dispose();

    private sealed class ApiEnvelope<T>
    {
        public string Status { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public T? Data { get; init; }
    }
}
