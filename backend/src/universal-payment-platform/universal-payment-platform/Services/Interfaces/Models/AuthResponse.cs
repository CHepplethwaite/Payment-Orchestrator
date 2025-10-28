namespace universal_payment_platform.Services.Interfaces.Models
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = string.Empty ;
        public string Token { get; set; } = string.Empty;
        public int ExpiresIn { get; set; } = 0;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime TokenExpiry { get; set; }
    }
}