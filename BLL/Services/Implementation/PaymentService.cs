using BLL.DTOs;
using DAL.Models;
using DAL.Repositories;

namespace BLL.Services.Implementation;

public class PaymentService : IPaymentService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly IPayOSService _payOSService;

    public PaymentService(
        ITransactionRepository transactionRepository,
        IPackageRepository packageRepository,
        IPayOSService payOSService)
    {
        _transactionRepository = transactionRepository;
        _packageRepository = packageRepository;
        _payOSService = payOSService;
    }

    public async Task<ServiceResult<TransactionDto>> CreateTransactionAsync(CreateTransactionDto dto)
    {
        var transaction = new Transaction
        {
            UserId = dto.UserId,
            PackageId = dto.PackageId,
            TransactionType = dto.TransactionType,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            Status = "Pending",
            Notes = dto.Notes
        };

        var created = await _transactionRepository.CreateTransactionAsync(transaction);
        return ServiceResult<TransactionDto>.SuccessResult(MapToDto(created), "Transaction created successfully");
    }

    public async Task<ServiceResult<TransactionDto>> GetTransactionByIdAsync(Guid id)
    {
        var transaction = await _transactionRepository.GetTransactionWithDetailsAsync(id);
        if (transaction == null)
            return ServiceResult<TransactionDto>.FailureResult("Transaction not found");

        return ServiceResult<TransactionDto>.SuccessResult(MapToDto(transaction));
    }

    public async Task<ServiceResult<List<TransactionDto>>> GetUserTransactionsAsync(Guid userId)
    {
        var transactions = await _transactionRepository.GetUserTransactionsAsync(userId);
        var dtos = transactions.Select(MapToDto).ToList();
        return ServiceResult<List<TransactionDto>>.SuccessResult(dtos);
    }

    public async Task<ServiceResult<List<TransactionDto>>> GetAllTransactionsAsync()
    {
        var transactions = await _transactionRepository.GetAllTransactionsAsync();
        var dtos = transactions.Select(MapToDto).ToList();
        return ServiceResult<List<TransactionDto>>.SuccessResult(dtos);
    }

    public async Task<ServiceResult<TransactionDto>> CompleteTransactionAsync(CompleteTransactionDto dto)
    {
        var transaction = await _transactionRepository.GetTransactionByIdAsync(dto.TransactionId);
        if (transaction == null)
            return ServiceResult<TransactionDto>.FailureResult("Transaction not found");

        if (transaction.Status != "Pending")
            return ServiceResult<TransactionDto>.FailureResult("Transaction is not pending");

        transaction.Status = "Completed";
        transaction.CompletedAt = DateTime.UtcNow;
        transaction.PaymentReference = dto.PaymentReference;
        
        if (!string.IsNullOrEmpty(dto.Notes))
            transaction.Notes = (transaction.Notes ?? "") + " | " + dto.Notes;

        await _transactionRepository.UpdateTransactionAsync(transaction);

        return ServiceResult<TransactionDto>.SuccessResult(
            MapToDto(transaction), 
            "Transaction completed successfully");
    }

    public async Task<ServiceResult<TransactionDto>> FailTransactionAsync(Guid transactionId, string reason)
    {
        var transaction = await _transactionRepository.GetTransactionByIdAsync(transactionId);
        if (transaction == null)
            return ServiceResult<TransactionDto>.FailureResult("Transaction not found");

        transaction.Status = "Failed";
        transaction.Notes = (transaction.Notes ?? "") + " | Failed: " + reason;

        await _transactionRepository.UpdateTransactionAsync(transaction);

        return ServiceResult<TransactionDto>.SuccessResult(
            MapToDto(transaction), 
            "Transaction marked as failed");
    }

    public async Task<ServiceResult<TransactionDto>> RefundTransactionAsync(Guid transactionId, string reason)
    {
        var transaction = await _transactionRepository.GetTransactionByIdAsync(transactionId);
        if (transaction == null)
            return ServiceResult<TransactionDto>.FailureResult("Transaction not found");

        if (transaction.Status != "Completed")
            return ServiceResult<TransactionDto>.FailureResult("Only completed transactions can be refunded");

        transaction.Status = "Refunded";
        transaction.Notes = (transaction.Notes ?? "") + " | Refunded: " + reason;

        await _transactionRepository.UpdateTransactionAsync(transaction);

        return ServiceResult<TransactionDto>.SuccessResult(
            MapToDto(transaction), 
            "Transaction refunded successfully");
    }

    public async Task<ServiceResult<string>> InitiatePaymentAsync(Guid transactionId, string paymentMethod)
    {
        try
        {
            var transaction = await _transactionRepository.GetTransactionWithDetailsAsync(transactionId);
            if (transaction == null)
                return ServiceResult<string>.FailureResult("Transaction not found");

            if (transaction.Status != "Pending")
                return ServiceResult<string>.FailureResult("Transaction is already processed");

            // Get package details for payment description
            // PayOS has a 25 character limit for description
            var packageName = transaction.Package?.Name ?? "Package";
            var orderRef = transactionId.ToString().Substring(0, 8);
            
            // Keep description under 25 characters
            var description = $"{packageName}";
            if (description.Length > 25)
            {
                description = description.Substring(0, 22) + "...";
            }

            // Get user details
            var buyerName = transaction.User?.DisplayName;
            var buyerEmail = transaction.User?.Email;
            var buyerPhone = transaction.User?.Phone;

            // Create PayOS payment link
            var paymentResult = await _payOSService.CreatePaymentLinkAsync(
                transactionId,
                transaction.Amount,
                description,
                buyerName,
                buyerEmail,
                buyerPhone
            );

            // Debug logging
            Console.WriteLine($"[PaymentService] PayOS Response:");
            Console.WriteLine($"  OrderCode: {paymentResult.OrderCode}");
            Console.WriteLine($"  CheckoutUrl: {paymentResult.CheckoutUrl}");

            // Update transaction with PayOS information (without QR code)
            transaction.PayOSOrderCode = paymentResult.OrderCode;
            transaction.PayOSCheckoutUrl = paymentResult.CheckoutUrl;
            transaction.PayOSQrCode = null; // Not using QR code
            transaction.PaymentMethod = paymentMethod;
            
            
            await _transactionRepository.UpdateTransactionAsync(transaction);

            Console.WriteLine($"[PaymentService] Transaction updated with PayOS data");

            return ServiceResult<string>.SuccessResult(
                paymentResult.CheckoutUrl,
                "Payment link created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.FailureResult($"Failed to initiate payment: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> VerifyPaymentAsync(string paymentReference)
    {
        try
        {
            if (string.IsNullOrEmpty(paymentReference))
                return ServiceResult<bool>.FailureResult("Invalid payment reference");

            // PayOS verification is done via webhook
            // This method can be used for additional verification if needed
            return ServiceResult<bool>.SuccessResult(true, "Payment verified");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.FailureResult($"Payment verification failed: {ex.Message}");
        }
    }

    public async Task<ServiceResult<decimal>> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.MinValue;
        var end = endDate ?? DateTime.MaxValue;

        var transactions = await _transactionRepository.GetTransactionsByDateRangeAsync(start, end);
        var revenue = transactions
            .Where(t => t.Status == "Completed")
            .Sum(t => t.Amount);

        return ServiceResult<decimal>.SuccessResult(revenue);
    }

    public async Task<ServiceResult<Dictionary<string, int>>> GetTransactionStatisticsAsync()
    {
        var allTransactions = await _transactionRepository.GetAllTransactionsAsync();
        
        var stats = new Dictionary<string, int>
        {
            ["Total"] = allTransactions.Count,
            ["Pending"] = allTransactions.Count(t => t.Status == "Pending"),
            ["Completed"] = allTransactions.Count(t => t.Status == "Completed"),
            ["Failed"] = allTransactions.Count(t => t.Status == "Failed"),
            ["Refunded"] = allTransactions.Count(t => t.Status == "Refunded")
        };

        return ServiceResult<Dictionary<string, int>>.SuccessResult(stats);
    }

    public async Task<ServiceResult<TransactionDto>> GetTransactionByPayOSOrderCodeAsync(long orderCode)
    {
        var transaction = await _transactionRepository.GetTransactionByPayOSOrderCodeAsync(orderCode);
        if (transaction == null)
            return ServiceResult<TransactionDto>.FailureResult("Transaction not found");

        return ServiceResult<TransactionDto>.SuccessResult(MapToDto(transaction));
    }

    public async Task<ServiceResult<TransactionDto>> UpdateTransactionPayOSReferenceAsync(Guid transactionId, string payOSTransactionId)
    {
        var transaction = await _transactionRepository.GetTransactionByIdAsync(transactionId);
        if (transaction == null)
            return ServiceResult<TransactionDto>.FailureResult("Transaction not found");

        transaction.PayOSTransactionId = payOSTransactionId;
        await _transactionRepository.UpdateTransactionAsync(transaction);

        return ServiceResult<TransactionDto>.SuccessResult(
            MapToDto(transaction),
            "Transaction PayOS reference updated successfully");
    }

    private TransactionDto MapToDto(Transaction transaction)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            UserId = transaction.UserId,
            PackageId = transaction.PackageId,
            TransactionType = transaction.TransactionType,
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            Status = transaction.Status,
            PaymentMethod = transaction.PaymentMethod,
            PaymentReference = transaction.PaymentReference,
            Notes = transaction.Notes,
            CreatedAt = transaction.CreatedAt,
            CompletedAt = transaction.CompletedAt
        };
    }
}
