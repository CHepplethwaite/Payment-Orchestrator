namespace universal_payment_platform.Services.Interfaces.Models
{
    public class AuthResponse
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; }
        public DateTime TokenExpiry { get; set; }
    }
}