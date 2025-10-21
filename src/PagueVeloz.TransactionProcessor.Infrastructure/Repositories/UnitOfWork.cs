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
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
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

