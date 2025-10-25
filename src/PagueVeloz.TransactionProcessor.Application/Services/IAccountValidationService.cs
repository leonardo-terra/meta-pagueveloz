using PagueVeloz.TransactionProcessor.Domain.Entities;

namespace PagueVeloz.TransactionProcessor.Application.Services;

public interface IAccountValidationService
{
    Task<bool> IsAccountValidAsync(Guid accountId);
    Task<bool> IsAccountActiveAsync(Guid accountId);
    Task<bool> IsClientActiveAsync(Guid clientId);
    Task<bool> HasSufficientBalanceAsync(Guid accountId, decimal amount);
    Task<bool> HasSufficientCreditLimitAsync(Guid clientId, decimal amount);
    Task<ValidationResult> ValidateAccountForTransactionAsync(Guid accountId, decimal amount, OperationType operation);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;

    public static ValidationResult Success() => new() { IsValid = true };
    
    public static ValidationResult Failure(string errorMessage, string errorCode = "VALIDATION_ERROR") => 
        new() { IsValid = false, ErrorMessage = errorMessage, ErrorCode = errorCode };
}
