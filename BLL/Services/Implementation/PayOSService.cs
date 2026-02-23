using BLL.Services;
using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace BLL.Services.Implementation;

public class PayOSService : IPayOSService
{
    private readonly PayOSClient _payOSClient;
    private readonly string cancelUrl;
    private readonly string returnUrl;
    private readonly string? webhookUrl;

    public PayOSService(IConfiguration configuration)
    {
        _payOSClient = new PayOSClient(new PayOSOptions
        {
            ClientId = configuration["PayOS:ClientId"] ?? Environment.GetEnvironmentVariable("PAYOS_CLIENT_ID"),
            ApiKey = configuration["PayOS:ApiKey"] ?? Environment.GetEnvironmentVariable("PAYOS_API_KEY"),
            ChecksumKey = configuration["PayOS:ChecksumKey"] ?? Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY"),
        });
        cancelUrl = configuration["PayOS:CancelUrl"];
        returnUrl = configuration["PayOS:ReturnUrl"];
        webhookUrl = configuration["PayOS:WebhookUrl"];
    }

    public async Task<PayOSPaymentResult> CreatePaymentLinkAsync(
        Guid transactionId,
        decimal amount,
        string description,
        string? buyerName = null,
        string? buyerEmail = null,
        string? buyerPhone = null)
    {
        try
        {
            // PayOS requires order code to be a positive integer
            // Use timestamp + random component to ensure uniqueness
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var random = new Random().Next(1000, 9999);
            var finalOrderCode = long.Parse($"{timestamp}{random}");

            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = finalOrderCode,
                Amount = (long)amount,
                Description = description,
                CancelUrl = cancelUrl,
                ReturnUrl = returnUrl + "?transactionId=" + transactionId.ToString()
            };

            var paymentLink = await _payOSClient.PaymentRequests.CreateAsync(paymentRequest);

            Console.WriteLine($"[PayOSService] Payment link created:");
            Console.WriteLine($"  OrderCode: {paymentLink.OrderCode}");
            Console.WriteLine($"  CheckoutUrl: {paymentLink.CheckoutUrl}");

            return new PayOSPaymentResult
            {
                OrderCode = paymentLink.OrderCode,
                CheckoutUrl = paymentLink.CheckoutUrl,
                QrCode = string.Empty // Not using QR code functionality
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create PayOS payment link: {ex.Message}", ex);
        }
    }

    public async Task<PayOSWebhookInfo?> VerifyWebhookDataAsync(Webhook webhookData)
    {
        try
        {
            // Verify webhook using PayOS SDK
            var verifiedData = await _payOSClient.Webhooks.VerifyAsync(webhookData);

            return new PayOSWebhookInfo
            {
                Code = verifiedData.Code,
                OrderCode = verifiedData.OrderCode,
                Reference = verifiedData.Reference,
                Amount = verifiedData.Amount
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PayOS] Webhook verification failed: {ex.Message}");
            return null;
        }
    }

    public async Task<PayOSPaymentInfo?> GetPaymentInfoAsync(long orderCode)
    {
        try
        {
            var paymentLink = await _payOSClient.PaymentRequests.GetAsync(orderCode);

            return new PayOSPaymentInfo
            {
                OrderCode = paymentLink.OrderCode,
                Amount = paymentLink.Amount,
                Status = paymentLink.Status.ToString()
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> CancelPaymentAsync(long orderCode, string? reason = null)
    {
        try
        {
            await _payOSClient.PaymentRequests.CancelAsync(orderCode, reason);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
