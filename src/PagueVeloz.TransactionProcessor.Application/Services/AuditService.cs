using Microsoft.Extensions.Logging;
using PagueVeloz.TransactionProcessor.Domain.Entities;

namespace PagueVeloz.TransactionProcessor.Application.Services;

public class AuditService : IAuditService
{
    private readonly ILogger<AuditService> _logger;

    public AuditService(ILogger<AuditService> logger)
    {
        _logger = logger;
    }

    public async Task LogAccountCreationAsync(Guid accountId, Guid clientId, decimal initialBalance, decimal creditLimit, string userIp, string userAgent)
    {
        var logEntry = new AuditLogEntry
        {
            EventType = "ACCOUNT_CREATED",
            AccountId = accountId,
            Details = $"Account created for Client {clientId}. Initial Balance: {initialBalance:C}, Credit Limit: {creditLimit:C}",
            UserIp = userIp,
            UserAgent = userAgent,
            CorrelationId = Guid.NewGuid().ToString()
        };

        _logger.LogInformation("AUDIT: {EventType} - AccountId: {AccountId}, ClientId: {ClientId}, InitialBalance: {InitialBalance}, CreditLimit: {CreditLimit}, UserIP: {UserIP}",
            logEntry.EventType, accountId, clientId, initialBalance, creditLimit, userIp);

        await Task.CompletedTask;
    }

    public async Task LogTransactionAttemptAsync(Guid accountId, string referenceId, OperationType operation, decimal amount, string currency, string userIp, string userAgent)
    {
        var logEntry = new AuditLogEntry
        {
            EventType = "TRANSACTION_ATTEMPT",
            AccountId = accountId,
            ReferenceId = referenceId,
            Details = $"Transaction attempt - Operation: {operation}, Amount: {amount:C} {currency}",
            UserIp = userIp,
            UserAgent = userAgent,
            CorrelationId = Guid.NewGuid().ToString()
        };

        _logger.LogInformation("AUDIT: {EventType} - AccountId: {AccountId}, ReferenceId: {ReferenceId}, Operation: {Operation}, Amount: {Amount}, Currency: {Currency}, UserIP: {UserIP}",
            logEntry.EventType, accountId, referenceId, operation, amount, currency, userIp);

        await Task.CompletedTask;
    }

    public async Task LogTransactionSuccessAsync(Guid transactionId, Guid accountId, string referenceId, OperationType operation, decimal amount, string currency)
    {
        var logEntry = new AuditLogEntry
        {
            EventType = "TRANSACTION_SUCCESS",
            TransactionId = transactionId,
            AccountId = accountId,
            ReferenceId = referenceId,
            Details = $"Transaction successful - Operation: {operation}, Amount: {amount:C} {currency}",
            CorrelationId = Guid.NewGuid().ToString()
        };

        _logger.LogInformation("AUDIT: {EventType} - TransactionId: {TransactionId}, AccountId: {AccountId}, ReferenceId: {ReferenceId}, Operation: {Operation}, Amount: {Amount}, Currency: {Currency}",
            logEntry.EventType, transactionId, accountId, referenceId, operation, amount, currency);

        await Task.CompletedTask;
    }

    public async Task LogTransactionFailureAsync(Guid accountId, string referenceId, OperationType operation, decimal amount, string currency, string errorMessage, string errorCode)
    {
        var logEntry = new AuditLogEntry
        {
            EventType = "TRANSACTION_FAILURE",
            AccountId = accountId,
            ReferenceId = referenceId,
            Details = $"Transaction failed - Operation: {operation}, Amount: {amount:C} {currency}, Error: {errorMessage}",
            CorrelationId = Guid.NewGuid().ToString()
        };

        _logger.LogWarning("AUDIT: {EventType} - AccountId: {AccountId}, ReferenceId: {ReferenceId}, Operation: {Operation}, Amount: {Amount}, Currency: {Currency}, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
            logEntry.EventType, accountId, referenceId, operation, amount, currency, errorCode, errorMessage);

        await Task.CompletedTask;
    }

    public async Task LogAccountStatusChangeAsync(Guid accountId, AccountStatus oldStatus, AccountStatus newStatus, string reason, string userIp)
    {
        var logEntry = new AuditLogEntry
        {
            EventType = "ACCOUNT_STATUS_CHANGE",
            AccountId = accountId,
            Details = $"Account status changed from {oldStatus} to {newStatus}. Reason: {reason}",
            UserIp = userIp,
            CorrelationId = Guid.NewGuid().ToString()
        };

        _logger.LogInformation("AUDIT: {EventType} - AccountId: {AccountId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}, Reason: {Reason}, UserIP: {UserIP}",
            logEntry.EventType, accountId, oldStatus, newStatus, reason, userIp);

        await Task.CompletedTask;
    }

    public async Task LogSuspiciousActivityAsync(Guid accountId, string activity, string details, string userIp, string userAgent)
    {
        var logEntry = new AuditLogEntry
        {
            EventType = "SUSPICIOUS_ACTIVITY",
            AccountId = accountId,
            Details = $"Suspicious activity detected - {activity}: {details}",
            UserIp = userIp,
            UserAgent = userAgent,
            CorrelationId = Guid.NewGuid().ToString()
        };

        _logger.LogWarning("AUDIT: {EventType} - AccountId: {AccountId}, Activity: {Activity}, Details: {Details}, UserIP: {UserIP}, UserAgent: {UserAgent}",
            logEntry.EventType, accountId, activity, details, userIp, userAgent);

        await Task.CompletedTask;
    }

    public async Task LogValidationFailureAsync(string validationType, string details, string userIp, string userAgent)
    {
        var logEntry = new AuditLogEntry
        {
            EventType = "VALIDATION_FAILURE",
            Details = $"Validation failed - Type: {validationType}, Details: {details}",
            UserIp = userIp,
            UserAgent = userAgent,
            CorrelationId = Guid.NewGuid().ToString()
        };

        _logger.LogWarning("AUDIT: {EventType} - ValidationType: {ValidationType}, Details: {Details}, UserIP: {UserIP}, UserAgent: {UserAgent}",
            logEntry.EventType, validationType, details, userIp, userAgent);

        await Task.CompletedTask;
    }
}
