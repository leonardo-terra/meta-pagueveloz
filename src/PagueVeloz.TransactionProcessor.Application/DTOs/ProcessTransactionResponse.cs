namespace PagueVeloz.TransactionProcessor.Application.DTOs;

public class ProcessTransactionResponse
{
    public Guid TransactionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal ReservedBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public DateTime Timestamp { get; set; }
    public string? ErrorMessage { get; set; }
}

