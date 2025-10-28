using System.Text;
using System.Text.Json;
using universal_payment_platform.Services.Interfaces;
using universal_payment_platform.Services.Interfaces.Models;

namespace universal_payment_platform.Services.ThirdPartyBankAdapters
{
    public class AirtelAdapter : IPaymentAdapter
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _baseUrl;
        private AuthResponse _authToken;
        private readonly ILogger<AirtelAdapter> _logger;

        public AirtelAdapter(HttpClient httpClient, IConfiguration configuration, ILogger<AirtelAdapter> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Load Airtel-specific configuration
            var airtelConfig = configuration.GetSection("PaymentProviders:Airtel");
            _clientId = airtelConfig["ClientId"];
            _clientSecret = airtelConfig["ClientSecret"];
            _baseUrl = airtelConfig["BaseUrl"];
        }

        public string GetAdapterName() => "Airtel";

        public async Task<AuthResponse> AuthenticateAsync()
        {
            try
            {
                var authData = new
                {
                    client_id = _clientId,
                    client_secret = _clientSecret,
                    grant_type = "client_credentials"
                };

                var content = new StringContent(JsonSerializer.Serialize(authData), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/auth/oauth2/token", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var authResponse = JsonSerializer.Deserialize<AirtelAuthResponse>(responseContent);

                    _authToken = new AuthResponse
                    {
                        AccessToken = authResponse.access_token,
                        TokenType = authResponse.token_type,
                        ExpiresIn = authResponse.expires_in,
                        TokenExpiry = DateTime.UtcNow.AddSeconds(authResponse.expires_in - 60) // Subtract 60 seconds for safety
                    };

                    _logger.LogInformation("Successfully authenticated with Airtel");
                    return _authToken;
                }

                throw new Exception($"Authentication failed: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Airtel authentication failed");
                throw;
            }
        }

        public async Task<PaymentResponse> MakePaymentAsync(PaymentRequest request)
        {
            try
            {
                if (_authToken == null || DateTime.UtcNow >= _authToken.TokenExpiry)
                {
                    await AuthenticateAsync();
                }

                var paymentData = new
                {
                    reference = request.Reference,
                    subscriber = new
                    {
                        country = "ZM",
                        currency = request.Currency,
                        msisdn = request.CustomerMSISDN
                    },
                    transaction = new
                    {
                        amount = request.Amount,
                        country = "ZM",
                        currency = request.Currency,
                        id = request.TransactionId
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(paymentData), Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken.AccessToken);

                var response = await _httpClient.PostAsync($"{_baseUrl}/merchant/v1/payments/", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var airtelResponse = JsonSerializer.Deserialize<AirtelPaymentResponse>(responseContent);

                    return new PaymentResponse
                    {
                        IsSuccess = airtelResponse.data.status == "SUCCESS",
                        TransactionId = request.TransactionId,
                        Reference = request.Reference,
                        Message = airtelResponse.data.status,
                        StatusCode = airtelResponse.status,
                        Timestamp = DateTime.UtcNow,
                        AdditionalData = new Dictionary<string, object>
                        {
                            { "airtel_transaction_id", airtelResponse.data.transaction.id },
                            { "airtel_status", airtelResponse.data.status },
                            { "airtel_message", airtelResponse.message }
                        }
                    };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<AirtelErrorResponse>(responseContent);
                    return new PaymentResponse
                    {
                        IsSuccess = false,
                        TransactionId = request.TransactionId,
                        Reference = request.Reference,
                        Message = errorResponse?.error?.message ?? "Payment failed",
                        StatusCode = response.StatusCode.ToString(),
                        Timestamp = DateTime.UtcNow
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Airtel payment failed for transaction {TransactionId}", request.TransactionId);
                return new PaymentResponse
                {
                    IsSuccess = false,
                    TransactionId = request.TransactionId,
                    Reference = request.Reference,
                    Message = $"Payment failed: {ex.Message}",
                    StatusCode = "500",
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task<PaymentResponse> CheckTransactionStatusAsync(string transactionId)
        {
            try
            {
                if (_authToken == null || DateTime.UtcNow >= _authToken.TokenExpiry)
                {
                    await AuthenticateAsync();
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken.AccessToken);

                var response = await _httpClient.GetAsync($"{_baseUrl}/standard/v1/payments/{transactionId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var statusResponse = JsonSerializer.Deserialize<AirtelStatusResponse>(responseContent);

                    return new PaymentResponse
                    {
                        IsSuccess = statusResponse.data.transaction.status == "TS",
                        TransactionId = transactionId,
                        Message = statusResponse.data.transaction.message,
                        StatusCode = statusResponse.status,
                        Timestamp = DateTime.UtcNow,
                        AdditionalData = new Dictionary<string, object>
                        {
                            { "airtel_status", statusResponse.data.transaction.status },
                            { "airtel_message", statusResponse.data.transaction.message }
                        }
                    };
                }

                return new PaymentResponse
                {
                    IsSuccess = false,
                    TransactionId = transactionId,
                    Message = "Failed to retrieve transaction status",
                    StatusCode = response.StatusCode.ToString(),
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check Airtel transaction status for {TransactionId}", transactionId);
                return new PaymentResponse
                {
                    IsSuccess = false,
                    TransactionId = transactionId,
                    Message = $"Status check failed: {ex.Message}",
                    StatusCode = "500",
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        // Airtel-specific response classes
        private class AirtelAuthResponse
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
        }

        private class AirtelPaymentResponse
        {
            public AirtelPaymentData data { get; set; }
            public string status { get; set; }
            public string message { get; set; }
        }

        private class AirtelPaymentData
        {
            public AirtelTransaction transaction { get; set; }
            public string status { get; set; }
        }

        private class AirtelTransaction
        {
            public string id { get; set; }
            public string status { get; set; }
        }

        private class AirtelErrorResponse
        {
            public AirtelError error { get; set; }
        }

        private class AirtelError
        {
            public string message { get; set; }
            public string code { get; set; }
        }

        private class AirtelStatusResponse
        {
            public AirtelStatusData data { get; set; }
            public string status { get; set; }
        }

        private class AirtelStatusData
        {
            public AirtelStatusTransaction transaction { get; set; }
        }

        private class AirtelStatusTransaction
        {
            public string status { get; set; }
            public string message { get; set; }
        }
    }
}