using Microsoft.EntityFrameworkCore.Storage;
using PagueVeloz.TransactionProcessor.Domain.Entities;
using PagueVeloz.TransactionProcessor.Domain.Interfaces;
using PagueVeloz.TransactionProcessor.Infrastructure.Data;

namespace PagueVeloz.TransactionProcessor.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly TransactionProcessorDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(TransactionProcessorDbContext context)
    {
        _context = context;
        Accounts = new AccountRepository(_context);
        Transactions = new TransactionRepository(_context);
        Clients = new Repository<Client>(_context);
    }

    public IAccountRepository Accounts { get; }
    public ITransactionRepository Transactions { get; }
    public IRepository<Client> Clients { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        // Check if we're using InMemory database (which doesn't support transactions)
        if (_context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            // For InMemory database, we'll simulate transaction behavior
            // by not actually creating a transaction
            return;
        }
        
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        // Check if we're using InMemory database (which doesn't support transactions)
        if (_context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            // For InMemory database, just save changes
            await _context.SaveChangesAsync();
            return;
        }
        
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        // Check if we're using InMemory database (which doesn't support transactions)
        if (_context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            // For InMemory database, we can't rollback, but we can clear the context
            // This is a limitation of InMemory database for testing
            return;
        }
        
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}

