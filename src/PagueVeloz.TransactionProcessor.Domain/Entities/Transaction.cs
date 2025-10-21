using System.ComponentModel.DataAnnotations;

namespace PagueVeloz.TransactionProcessor.Domain.Entities;

public class Transaction
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid AccountId { get; set; }
    
    public string ReferenceId { get; set; } = string.Empty;
    
    public OperationType Operation { get; set; }
    
    public decimal Amount { get; set; }
    
    public string Currency { get; set; } = "BRL";
    
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    
    public string? ErrorMessage { get; set; }
    
    public string? Metadata { get; set; } // JSON string for additional data
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ProcessedAt { get; set; }
    
    // Navigation properties
    public virtual Account Account { get; set; } = null!;
    
    // Business methods
    public void MarkAsSuccess()
    {
        Status = TransactionStatus.Success;
        ProcessedAt = DateTime.UtcNow;
        ErrorMessage = null;
    }
    
    public void MarkAsFailed(string errorMessage)
    {
        Status = TransactionStatus.Failed;
        ProcessedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }
}

public enum OperationType
{
    Credit,
    Debit,
    Reserve,
    Capture,
    Reversal,
    Transfer
}

public enum TransactionStatus
{
    Pending,
    Success,
    Failed
}

