using System.Text;
using System.Text.Json;

namespace WebBanNuocMVC.DesignPatterns.FactoryMethod;

public class PayPalService : IPaymentService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public PayPalService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
    }

    public string CreatePaymentUrl(int orderId, decimal amount, string orderInfo, string returnUrl)
    {
        var clientId = _configuration["PayPal:ClientId"];
        var clientSecret = _configuration["PayPal:ClientSecret"];
        var mode = _configuration["PayPal:Mode"];

        var baseUrl = mode == "sandbox"
            ? "https://api-m.sandbox.paypal.com"
            : "https://api-m.paypal.com";

        var accessToken = GetAccessToken(baseUrl, clientId, clientSecret).Result;

        var order = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                    new
                    {
                        reference_id = orderId.ToString(),
                        description = orderInfo,
                        amount = new
                        {
                            currency_code = "USD",
                            value = amount.ToString("F2")
                        }
                    }
                },
            application_context = new
            {
                return_url = returnUrl,
                cancel_url = returnUrl.Replace("Success", "Failed")
            }
        };

        var json = JsonSerializer.Serialize(order);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        var response = _httpClient.PostAsync($"{baseUrl}/v2/checkout/orders", content).Result;
        var responseContent = response.Content.ReadAsStringAsync().Result;

        var orderResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
        var approveLink = orderResponse.GetProperty("links")
            .EnumerateArray()
            .FirstOrDefault(l => l.GetProperty("rel").GetString() == "approve")
            .GetProperty("href").GetString();

        return approveLink ?? "";
    }

    public bool ValidateCallback(Dictionary<string, string> queryParams)
    {
        if (!queryParams.ContainsKey("token"))
            return false;

        var token = queryParams["token"];
        var clientId = _configuration["PayPal:ClientId"];
        var clientSecret = _configuration["PayPal:ClientSecret"];
        var mode = _configuration["PayPal:Mode"];

        var baseUrl = mode == "sandbox"
            ? "https://api-m.sandbox.paypal.com"
            : "https://api-m.paypal.com";

        var accessToken = GetAccessToken(baseUrl, clientId, clientSecret).Result;

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        var response = _httpClient.GetAsync($"{baseUrl}/v2/checkout/orders/{token}").Result;

        return response.IsSuccessStatusCode;
    }

    private async Task<string> GetAccessToken(string baseUrl, string clientId, string clientSecret)
    {
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {auth}");

        var content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");
        var response = await _httpClient.PostAsync($"{baseUrl}/v1/oauth2/token", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
        return tokenResponse.GetProperty("access_token").GetString() ?? "";
    }
}

