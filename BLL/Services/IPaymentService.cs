using BLL.DTOs;
using DAL.Models;

namespace BLL.Services;

public interface IPaymentService
{
    // Transaction operations
    Task<ServiceResult<TransactionDto>> CreateTransactionAsync(CreateTransactionDto dto);
    Task<ServiceResult<TransactionDto>> GetTransactionByIdAsync(Guid id);
    Task<ServiceResult<List<TransactionDto>>> GetUserTransactionsAsync(Guid userId);
    Task<ServiceResult<List<TransactionDto>>> GetAllTransactionsAsync();
    Task<ServiceResult<TransactionDto>> CompleteTransactionAsync(CompleteTransactionDto dto);
    Task<ServiceResult<TransactionDto>> FailTransactionAsync(Guid transactionId, string reason);
    Task<ServiceResult<TransactionDto>> RefundTransactionAsync(Guid transactionId, string reason);

    // Payment processing (can be extended for actual payment gateway integration)
    Task<ServiceResult<string>> InitiatePaymentAsync(Guid transactionId, string paymentMethod);
    Task<ServiceResult<bool>> VerifyPaymentAsync(string paymentReference);

    // Reporting
    Task<ServiceResult<decimal>> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ServiceResult<Dictionary<string, int>>> GetTransactionStatisticsAsync();
    
    // PayOS specific operations
    Task<ServiceResult<TransactionDto>> GetTransactionByPayOSOrderCodeAsync(long orderCode);
    Task<ServiceResult<TransactionDto>> UpdateTransactionPayOSReferenceAsync(Guid transactionId, string payOSTransactionId);
}
