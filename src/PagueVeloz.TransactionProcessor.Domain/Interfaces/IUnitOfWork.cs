using PagueVeloz.TransactionProcessor.Domain.Entities;

namespace PagueVeloz.TransactionProcessor.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IAccountRepository Accounts { get; }
    ITransactionRepository Transactions { get; }
    IRepository<Client> Clients { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

