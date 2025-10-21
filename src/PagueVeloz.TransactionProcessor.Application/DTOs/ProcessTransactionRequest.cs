using PagueVeloz.TransactionProcessor.Domain.Entities;

namespace PagueVeloz.TransactionProcessor.Application.DTOs;

public class ProcessTransactionRequest
{
    public OperationType Operation { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BRL";
    public string ReferenceId { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}

