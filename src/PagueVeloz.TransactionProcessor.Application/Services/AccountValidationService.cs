using PagueVeloz.TransactionProcessor.Domain.Entities;
using PagueVeloz.TransactionProcessor.Domain.Interfaces;

namespace PagueVeloz.TransactionProcessor.Application.Services;

public class AccountValidationService : IAccountValidationService
{
    private readonly IUnitOfWork _unitOfWork;

    public AccountValidationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> IsAccountValidAsync(Guid accountId)
    {
        var account = await _unitOfWork.Accounts.GetByAccountIdAsync(accountId);
        return account != null;
    }

    public async Task<bool> IsAccountActiveAsync(Guid accountId)
    {
        var account = await _unitOfWork.Accounts.GetByAccountIdAsync(accountId);
        return account?.Status == AccountStatus.Active;
    }

    public async Task<bool> IsClientActiveAsync(Guid clientId)
    {
        var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
        return client?.Status == ClientStatus.Active;
    }

    public async Task<bool> HasSufficientBalanceAsync(Guid accountId, decimal amount)
    {
        var account = await _unitOfWork.Accounts.GetByAccountIdAsync(accountId);
        if (account == null) return false;

        return account.AvailableBalance >= amount;
    }

    public async Task<bool> HasSufficientCreditLimitAsync(Guid clientId, decimal amount)
    {
        var accounts = await _unitOfWork.Accounts.GetByClientIdAsync(clientId);
        if (!accounts.Any()) return false;

        var totalCreditLimit = accounts.Sum(a => a.CreditLimit);
        var totalUsedCredit = accounts.Sum(a => a.Balance);
        
        return (totalCreditLimit - totalUsedCredit) >= amount;
    }

    public async Task<ValidationResult> ValidateAccountForTransactionAsync(Guid accountId, decimal amount, OperationType operation)
    {
        var account = await _unitOfWork.Accounts.GetByAccountIdAsync(accountId);
        if (account == null)
        {
            return ValidationResult.Failure("Conta não encontrada", "ACCOUNT_NOT_FOUND");
        }

        if (account.Status != AccountStatus.Active)
        {
            return ValidationResult.Failure("Conta não está ativa", "ACCOUNT_INACTIVE");
        }

        if (account.Client?.Status != ClientStatus.Active)
        {
            return ValidationResult.Failure("Cliente não está ativo", "CLIENT_INACTIVE");
        }

        switch (operation)
        {
            case OperationType.Debit:
                if (!account.CanDebit(amount))
                {
                    return ValidationResult.Failure(
                        $"Saldo insuficiente. Disponível: {account.TotalAvailableBalance:C}, Solicitado: {amount:C}",
                        "INSUFFICIENT_BALANCE");
                }
                break;

            case OperationType.Reserve:
                if (!account.CanReserve(amount))
                {
                    return ValidationResult.Failure(
                        $"Saldo disponível insuficiente para reserva. Disponível: {account.AvailableBalance:C}, Solicitado: {amount:C}",
                        "INSUFFICIENT_AVAILABLE_BALANCE");
                }
                break;

            case OperationType.Credit:
                var totalCreditUsed = account.Balance;
                if (totalCreditUsed + amount > account.CreditLimit)
                {
                    return ValidationResult.Failure(
                        $"Operação excederia o limite de crédito. Limite: {account.CreditLimit:C}, Usado: {totalCreditUsed:C}, Solicitado: {amount:C}",
                        "CREDIT_LIMIT_EXCEEDED");
                }
                break;

            case OperationType.Capture:
                if (!account.CanCapture(amount))
                {
                    return ValidationResult.Failure(
                        $"Valor reservado insuficiente para captura. Reservado: {account.ReservedBalance:C}, Solicitado: {amount:C}",
                        "INSUFFICIENT_RESERVED_BALANCE");
                }
                break;

            case OperationType.Reversal:
                var transactions = await _unitOfWork.Transactions.GetByAccountIdAndStatusAsync(accountId, TransactionStatus.Success);
                if (!transactions.Any())
                {
                    return ValidationResult.Failure("Não há transações para reverter", "NO_TRANSACTIONS_TO_REVERSE");
                }
                break;

            case OperationType.Transfer:
                if (!account.CanDebit(amount))
                {
                    return ValidationResult.Failure(
                        $"Saldo insuficiente para transferência. Disponível: {account.TotalAvailableBalance:C}, Solicitado: {amount:C}",
                        "INSUFFICIENT_BALANCE");
                }
                break;
        }

        return ValidationResult.Success();
    }
}
