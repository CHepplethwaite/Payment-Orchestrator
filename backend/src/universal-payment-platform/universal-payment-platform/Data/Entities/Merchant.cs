namespace universal_payment_platform.Data.Entities
{
    public class Merchant
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public virtual ICollection<SettlementAccount> SettlementAccounts { get; set; } = new List<SettlementAccount>();
    }
}