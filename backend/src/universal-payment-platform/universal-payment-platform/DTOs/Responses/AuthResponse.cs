namespace universal_payment_platform.DTOs.Responses
{
    public class AuthResponse
    {
        public required string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
