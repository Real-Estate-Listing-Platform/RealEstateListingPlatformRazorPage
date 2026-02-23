
using PayOS.Models.Webhooks;

namespace BLL.Services;

public interface IPayOSService
{
    /// <summary>
    /// Creates a payment link for a transaction
    /// </summary>
    Task<PayOSPaymentResult> CreatePaymentLinkAsync(
        Guid transactionId, 
        decimal amount, 
        string description,
        string? buyerName = null,
        string? buyerEmail = null,
        string? buyerPhone = null);

    /// <summary>
    /// Verifies webhook data using PayOS SDK
    /// </summary>
    Task<PayOSWebhookInfo?> VerifyWebhookDataAsync(Webhook webhookData);

    /// <summary>
    /// Gets payment information by order code
    /// </summary>
    Task<PayOSPaymentInfo?> GetPaymentInfoAsync(long orderCode);

    /// <summary>
    /// Cancels a payment link
    /// </summary>
    Task<bool> CancelPaymentAsync(long orderCode, string? reason = null);
}

public class PayOSPaymentResult
{
    public long OrderCode { get; set; }
    public string CheckoutUrl { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;
}

public class PayOSPaymentInfo
{
    public long OrderCode { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
}

public class PayOSWebhookInfo
{
    public string Code { get; set; } = string.Empty;
    public long OrderCode { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public decimal Amount { get; set; }
}
