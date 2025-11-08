namespace universal_payment_platform.DTOs.@public
{
    public class AuthResponse
    {
        public required string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
