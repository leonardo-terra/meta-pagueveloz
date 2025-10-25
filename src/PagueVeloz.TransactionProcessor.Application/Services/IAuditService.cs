using PagueVeloz.TransactionProcessor.Domain.Entities;

namespace PagueVeloz.TransactionProcessor.Application.Services;

public interface IAuditService
{
    Task LogAccountCreationAsync(Guid accountId, Guid clientId, decimal initialBalance, decimal creditLimit, string userIp, string userAgent);
    Task LogTransactionAttemptAsync(Guid accountId, string referenceId, OperationType operation, decimal amount, string currency, string userIp, string userAgent);
    Task LogTransactionSuccessAsync(Guid transactionId, Guid accountId, string referenceId, OperationType operation, decimal amount, string currency);
    Task LogTransactionFailureAsync(Guid accountId, string referenceId, OperationType operation, decimal amount, string currency, string errorMessage, string errorCode);
    Task LogAccountStatusChangeAsync(Guid accountId, AccountStatus oldStatus, AccountStatus newStatus, string reason, string userIp);
    Task LogSuspiciousActivityAsync(Guid accountId, string activity, string details, string userIp, string userAgent);
    Task LogValidationFailureAsync(string validationType, string details, string userIp, string userAgent);
}

public class AuditLogEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public Guid? AccountId { get; set; }
    public Guid? TransactionId { get; set; }
    public string ReferenceId { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string UserIp { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string CorrelationId { get; set; } = string.Empty;
}
