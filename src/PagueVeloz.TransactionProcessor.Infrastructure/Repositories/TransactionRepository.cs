using Microsoft.EntityFrameworkCore;
using PagueVeloz.TransactionProcessor.Domain.Entities;
using PagueVeloz.TransactionProcessor.Domain.Interfaces;
using PagueVeloz.TransactionProcessor.Infrastructure.Data;

namespace PagueVeloz.TransactionProcessor.Infrastructure.Repositories;

public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(TransactionProcessorDbContext context) : base(context)
    {
    }

    public async Task<Transaction?> GetByReferenceIdAsync(string referenceId)
    {
        return await _dbSet
            .Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.ReferenceId == referenceId);
    }

    public async Task<bool> ExistsByReferenceIdAsync(string referenceId)
    {
        return await _dbSet.AnyAsync(t => t.ReferenceId == referenceId);
    }

    public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(Guid accountId)
    {
        return await _dbSet
            .Include(t => t.Account)
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetByAccountIdAndStatusAsync(Guid accountId, TransactionStatus status)
    {
        return await _dbSet
            .Include(t => t.Account)
            .Where(t => t.AccountId == accountId && t.Status == status)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
}

