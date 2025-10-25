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

    public async Task<Account?> GetByAccountIdForUpdateAsync(Guid accountId)
    {
        // Check if we're using InMemory database (which doesn't support FOR UPDATE)
        if (_context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            // For InMemory database, just return the account without locking
            // This is a limitation of InMemory database for testing
            return await _dbSet
                .Include(a => a.Client)
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.Id == accountId);
        }

        // Use raw SQL to implement pessimistic locking for real databases
        // This will lock the row for update until the transaction is committed
        var sql = @"
            SELECT a.*, c.* 
            FROM Accounts a 
            INNER JOIN Clients c ON a.ClientId = c.Id 
            WHERE a.Id = {0} 
            FOR UPDATE";

        var account = await _dbSet
            .FromSqlRaw(sql, accountId)
            .Include(a => a.Client)
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync();

        return account;
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

