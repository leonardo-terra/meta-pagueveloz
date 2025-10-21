using PagueVeloz.TransactionProcessor.Application.DTOs;

namespace PagueVeloz.TransactionProcessor.Application.Services;

public interface ITransactionService
{
    Task<ProcessTransactionResponse> ProcessTransactionAsync(ProcessTransactionRequest request);
}

