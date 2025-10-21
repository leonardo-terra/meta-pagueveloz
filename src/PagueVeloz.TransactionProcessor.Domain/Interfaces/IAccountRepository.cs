using PagueVeloz.TransactionProcessor.Domain.Entities;

namespace PagueVeloz.TransactionProcessor.Domain.Interfaces;

public interface IAccountRepository : IRepository<Account>
{
    Task<Account?> GetByAccountIdAsync(Guid accountId);
    Task<bool> ExistsByAccountIdAsync(Guid accountId);
    Task<IEnumerable<Account>> GetByClientIdAsync(Guid clientId);
}

