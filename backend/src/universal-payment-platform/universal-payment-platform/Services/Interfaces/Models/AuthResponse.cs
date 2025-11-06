namespace universal_payment_platform.Services.Interfaces.Models
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiry { get; set; }
    }
}