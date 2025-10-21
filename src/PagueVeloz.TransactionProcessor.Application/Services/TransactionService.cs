using PagueVeloz.TransactionProcessor.Application.DTOs;
using PagueVeloz.TransactionProcessor.Domain.Entities;
using PagueVeloz.TransactionProcessor.Domain.Interfaces;
using System.Text.Json;

namespace PagueVeloz.TransactionProcessor.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProcessTransactionResponse> ProcessTransactionAsync(ProcessTransactionRequest request)
    {
        // Check if transaction already exists (idempotency)
        var existingTransaction = await _unitOfWork.Transactions.GetByReferenceIdAsync(request.ReferenceId);
        if (existingTransaction != null)
        {
            return new ProcessTransactionResponse
            {
                TransactionId = existingTransaction.Id,
                Status = existingTransaction.Status.ToString().ToLower(),
                Balance = existingTransaction.Account.Balance,
                ReservedBalance = existingTransaction.Account.ReservedBalance,
                AvailableBalance = existingTransaction.Account.AvailableBalance,
                Timestamp = existingTransaction.ProcessedAt ?? existingTransaction.CreatedAt,
                ErrorMessage = existingTransaction.ErrorMessage
            };
        }

        // Get account
        var account = await _unitOfWork.Accounts.GetByAccountIdAsync(request.AccountId);
        if (account == null)
        {
            return new ProcessTransactionResponse
            {
                TransactionId = Guid.NewGuid(),
                Status = "failed",
                Balance = 0,
                ReservedBalance = 0,
                AvailableBalance = 0,
                Timestamp = DateTime.UtcNow,
                ErrorMessage = "Account not found"
            };
        }

        // Create transaction
        var transaction = new Transaction
        {
            AccountId = request.AccountId,
            ReferenceId = request.ReferenceId,
            Operation = request.Operation,
            Amount = request.Amount,
            Currency = request.Currency,
            Status = TransactionStatus.Pending,
            Metadata = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null
        };

        await _unitOfWork.Transactions.AddAsync(transaction);

        // For now, just return a basic response without processing the transaction
        // This will be implemented in the next phases
        transaction.MarkAsSuccess();
        await _unitOfWork.SaveChangesAsync();

        return new ProcessTransactionResponse
        {
            TransactionId = transaction.Id,
            Status = transaction.Status.ToString().ToLower(),
            Balance = account.Balance,
            ReservedBalance = account.ReservedBalance,
            AvailableBalance = account.AvailableBalance,
            Timestamp = transaction.ProcessedAt ?? transaction.CreatedAt,
            ErrorMessage = transaction.ErrorMessage
        };
    }
}

