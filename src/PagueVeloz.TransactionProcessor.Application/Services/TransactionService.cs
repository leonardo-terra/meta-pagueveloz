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
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var existingTransaction = await _unitOfWork.Transactions.GetByReferenceIdAsync(request.ReferenceId);
            if (existingTransaction != null)
            {
                await _unitOfWork.CommitTransactionAsync();
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

            var account = await _unitOfWork.Accounts.GetByAccountIdForUpdateAsync(request.AccountId);
            if (account == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
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

            await _unitOfWork.Transactions.AddAsync(transaction);

            try
            {
                await ProcessTransactionByType(transaction, account);
                
                transaction.MarkAsSuccess();
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

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
            catch (Exception ex)
            {
                transaction.MarkAsFailed(ex.Message);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

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
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            
            return new ProcessTransactionResponse
            {
                TransactionId = Guid.NewGuid(),
                Status = "failed",
                Balance = 0,
                ReservedBalance = 0,
                AvailableBalance = 0,
                Timestamp = DateTime.UtcNow,
                ErrorMessage = $"Transaction failed: {ex.Message}"
            };
        }
    }

    private async Task ProcessTransactionByType(Transaction transaction, Account account)
    {
        switch (transaction.Operation)
        {
            case OperationType.Credit:
                await ProcessCreditTransaction(transaction, account);
                break;

            case OperationType.Debit:
                await ProcessDebitTransaction(transaction, account);
                break;

            case OperationType.Reserve:
                await ProcessReserveTransaction(transaction, account);
                break;

            case OperationType.Capture:
                await ProcessCaptureTransaction(transaction, account);
                break;

            case OperationType.Reversal:
                await ProcessReversalTransaction(transaction, account);
                break;

            case OperationType.Transfer:
                await ProcessTransferTransaction(transaction, account);
                break;

            default:
                throw new InvalidOperationException($"Operação não suportada: {transaction.Operation}");
        }
    }

    private Task ProcessCreditTransaction(Transaction transaction, Account account)
    {
        account.Balance += transaction.Amount;
        account.UpdatedAt = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    private Task ProcessDebitTransaction(Transaction transaction, Account account)
    {
        if (!account.CanDebit(transaction.Amount))
        {
            throw new InvalidOperationException($"Saldo insuficiente. Disponível: {account.TotalAvailableBalance:C}, Solicitado: {transaction.Amount:C}");
        }

        account.Balance -= transaction.Amount;
        account.UpdatedAt = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    private Task ProcessReserveTransaction(Transaction transaction, Account account)
    {
        if (!account.CanReserve(transaction.Amount))
        {
            throw new InvalidOperationException($"Saldo disponível insuficiente para reserva. Disponível: {account.AvailableBalance:C}, Solicitado: {transaction.Amount:C}");
        }

        account.ReservedBalance += transaction.Amount;
        account.UpdatedAt = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    private Task ProcessCaptureTransaction(Transaction transaction, Account account)
    {
        if (!account.CanCapture(transaction.Amount))
        {
            throw new InvalidOperationException($"Valor reservado insuficiente para captura. Reservado: {account.ReservedBalance:C}, Solicitado: {transaction.Amount:C}");
        }

        account.ReservedBalance -= transaction.Amount;
        account.Balance -= transaction.Amount;
        account.UpdatedAt = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    private async Task ProcessReversalTransaction(Transaction transaction, Account account)
    {
        if (string.IsNullOrEmpty(transaction.Metadata))
        {
            throw new InvalidOperationException("Reversão requer referência à transação original");
        }

        var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(transaction.Metadata);
        if (metadata == null || !metadata.ContainsKey("original_reference_id"))
        {
            throw new InvalidOperationException("Reversão requer original_reference_id no metadata");
        }

        var originalReferenceId = metadata["original_reference_id"]?.ToString();
        if (string.IsNullOrEmpty(originalReferenceId))
        {
            throw new InvalidOperationException("ReferenceId original não pode ser vazio");
        }

        var originalTransaction = await _unitOfWork.Transactions.GetByReferenceIdAsync(originalReferenceId);
        if (originalTransaction == null)
        {
            throw new InvalidOperationException($"Transação original não encontrada: {originalReferenceId}");
        }

        if (originalTransaction.AccountId != account.Id)
        {
            throw new InvalidOperationException("Transação original não pertence a esta conta");
        }

        if (originalTransaction.Status != TransactionStatus.Success)
        {
            throw new InvalidOperationException("Apenas transações bem-sucedidas podem ser revertidas");
        }

        var existingReversal = await _unitOfWork.Transactions.FindAsync(t => 
            t.Operation == OperationType.Reversal && 
            t.Metadata != null && 
            t.Metadata.Contains(originalReferenceId));
        
        if (existingReversal.Any())
        {
            throw new InvalidOperationException("Transação já foi revertida anteriormente");
        }

        if (transaction.Amount != originalTransaction.Amount)
        {
            throw new InvalidOperationException("Valor da reversão deve ser igual ao valor da transação original");
        }

        switch (originalTransaction.Operation)
        {
            case OperationType.Credit:
                if (!account.CanDebit(transaction.Amount))
                {
                    throw new InvalidOperationException($"Saldo insuficiente para reverter crédito. Disponível: {account.TotalAvailableBalance:C}, Solicitado: {transaction.Amount:C}");
                }
                account.Balance -= transaction.Amount;
                break;

            case OperationType.Debit:
                account.Balance += transaction.Amount;
                break;

            case OperationType.Reserve:
                if (account.ReservedBalance < transaction.Amount)
                {
                    throw new InvalidOperationException($"Valor reservado insuficiente para reversão. Reservado: {account.ReservedBalance:C}, Solicitado: {transaction.Amount:C}");
                }
                account.ReservedBalance -= transaction.Amount;
                break;

            case OperationType.Capture:
                account.ReservedBalance += transaction.Amount;
                account.Balance += transaction.Amount;
                break;

            case OperationType.Transfer:
                account.Balance += transaction.Amount;
                break;

            default:
                throw new InvalidOperationException($"Tipo de transação original não suporta reversão: {originalTransaction.Operation}");
        }

        account.UpdatedAt = DateTime.UtcNow;
    }

    private async Task ProcessTransferTransaction(Transaction transaction, Account account)
    {
        if (string.IsNullOrEmpty(transaction.Metadata))
        {
            throw new InvalidOperationException("Transferência requer conta de destino especificada no metadata");
        }

        var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(transaction.Metadata);
        if (metadata == null || !metadata.ContainsKey("destination_account_id"))
        {
            throw new InvalidOperationException("Transferência requer conta de destino especificada no metadata");
        }

        var destinationAccountIdStr = metadata["destination_account_id"]?.ToString();
        if (!Guid.TryParse(destinationAccountIdStr, out var destinationAccountId))
        {
            throw new InvalidOperationException("ID da conta de destino inválido");
        }

        var destinationAccount = await _unitOfWork.Accounts.GetByAccountIdAsync(destinationAccountId);
        if (destinationAccount == null)
        {
            throw new InvalidOperationException("Conta de destino não encontrada");
        }

        if (!account.CanDebit(transaction.Amount))
        {
            throw new InvalidOperationException($"Saldo insuficiente para transferência. Disponível: {account.TotalAvailableBalance:C}, Solicitado: {transaction.Amount:C}");
        }

        if (destinationAccount.Status != AccountStatus.Active)
        {
            throw new InvalidOperationException("Conta de destino não está ativa");
        }

        if (destinationAccount.Client.Status != ClientStatus.Active)
        {
            throw new InvalidOperationException("Cliente da conta de destino não está ativo");
        }

        account.Balance -= transaction.Amount;
        account.UpdatedAt = DateTime.UtcNow;

        destinationAccount.Balance += transaction.Amount;
        destinationAccount.UpdatedAt = DateTime.UtcNow;
    }
}

