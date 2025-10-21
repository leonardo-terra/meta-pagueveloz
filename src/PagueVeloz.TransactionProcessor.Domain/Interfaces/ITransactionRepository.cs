using PagueVeloz.TransactionProcessor.Domain.Entities;

namespace PagueVeloz.TransactionProcessor.Domain.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<Transaction?> GetByReferenceIdAsync(string referenceId);
    Task<bool> ExistsByReferenceIdAsync(string referenceId);
    Task<IEnumerable<Transaction>> GetByAccountIdAsync(Guid accountId);
    Task<IEnumerable<Transaction>> GetByAccountIdAndStatusAsync(Guid accountId, TransactionStatus status);
}

