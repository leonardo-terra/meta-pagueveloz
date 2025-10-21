using Microsoft.EntityFrameworkCore;
using PagueVeloz.TransactionProcessor.Domain.Entities;
using PagueVeloz.TransactionProcessor.Domain.Interfaces;
using PagueVeloz.TransactionProcessor.Infrastructure.Data;

namespace PagueVeloz.TransactionProcessor.Infrastructure.Repositories;

public class AccountRepository : Repository<Account>, IAccountRepository
{
    public AccountRepository(TransactionProcessorDbContext context) : base(context)
    {
    }

    public async Task<Account?> GetByAccountIdAsync(Guid accountId)
    {
        return await _dbSet
            .Include(a => a.Client)
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Id == accountId);
    }

    public async Task<bool> ExistsByAccountIdAsync(Guid accountId)
    {
        return await _dbSet.AnyAsync(a => a.Id == accountId);
    }

    public async Task<IEnumerable<Account>> GetByClientIdAsync(Guid clientId)
    {
        return await _dbSet
            .Include(a => a.Client)
            .Where(a => a.ClientId == clientId)
            .ToListAsync();
    }
}

