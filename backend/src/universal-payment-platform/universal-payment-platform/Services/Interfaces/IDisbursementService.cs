using universal_payment_platform.Services.Interfaces.Models;
  

// Services/Interfaces/IDisbursementService.cs
namespace universal_payment_platform.Services.Interfaces
{
    public interface IDisbursementService
    {
        Task<DisbursementResponse> ProcessDisbursementAsync(DisbursementRequest request);
        Task<DisbursementResponse> GetDisbursementStatusAsync(string transactionId);
    }
}