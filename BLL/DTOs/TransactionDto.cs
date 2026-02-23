namespace BLL.DTOs;

public class TransactionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? PackageId { get; set; }
    public string TransactionType { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string Status { get; set; } = null!;
    public string? PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class CreateTransactionDto
{
    public Guid UserId { get; set; }
    public Guid? PackageId { get; set; }
    public string TransactionType { get; set; } = null!;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string? Notes { get; set; }
}

public class CompleteTransactionDto
{
    public Guid TransactionId { get; set; }
    public string PaymentReference { get; set; } = null!;
    public string? Notes { get; set; }
}
