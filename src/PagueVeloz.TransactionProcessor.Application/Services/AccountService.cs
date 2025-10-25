using PagueVeloz.TransactionProcessor.Application.DTOs;
using PagueVeloz.TransactionProcessor.Domain.Entities;
using PagueVeloz.TransactionProcessor.Domain.Interfaces;

namespace PagueVeloz.TransactionProcessor.Application.Services;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork _unitOfWork;

    public AccountService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateAccountResponse> CreateAccountAsync(CreateAccountRequest request)
    {
        // Business Rule: Validate initial balance against credit limit
        if (request.InitialBalance > request.CreditLimit)
        {
            throw new InvalidOperationException("Saldo inicial não pode ser maior que o limite de crédito");
        }

        // Business Rule: Validate credit limit is reasonable
        if (request.CreditLimit < 0 || request.CreditLimit > 10000000)
        {
            throw new InvalidOperationException("Limite de crédito deve estar entre 0 e R$ 10.000.000,00");
        }

        // Business Rule: Validate initial balance is reasonable
        if (request.InitialBalance < 0 || request.InitialBalance > 1000000)
        {
            throw new InvalidOperationException("Saldo inicial deve estar entre 0 e R$ 1.000.000,00");
        }

        // Create or get client
        var client = await _unitOfWork.Clients.GetByIdAsync(request.ClientId);
        if (client == null)
        {
            client = new Client
            {
                Id = request.ClientId,
                Name = $"Client {request.ClientId}",
                Email = $"client{request.ClientId}@example.com",
                Status = ClientStatus.Active
            };
            await _unitOfWork.Clients.AddAsync(client);
        }
        else
        {
            // Business Rule: Check if client is active
            if (client.Status != ClientStatus.Active)
            {
                throw new InvalidOperationException("Cliente deve estar ativo para criar uma conta");
            }
        }

        // Business Rule: Check if client already has too many accounts (optional limit)
        var existingAccounts = await _unitOfWork.Accounts.GetByClientIdAsync(request.ClientId);
        if (existingAccounts.Count() >= 10) // Business rule: max 10 accounts per client
        {
            throw new InvalidOperationException("Cliente já possui o número máximo de contas permitidas (10)");
        }

        // Create account
        var account = new Account
        {
            ClientId = request.ClientId,
            Balance = request.InitialBalance,
            ReservedBalance = 0,
            CreditLimit = request.CreditLimit,
            Status = AccountStatus.Active
        };

        await _unitOfWork.Accounts.AddAsync(account);
        await _unitOfWork.SaveChangesAsync();

        return new CreateAccountResponse
        {
            AccountId = account.Id,
            ClientId = account.ClientId,
            Balance = account.Balance,
            ReservedBalance = account.ReservedBalance,
            AvailableBalance = account.AvailableBalance,
            CreditLimit = account.CreditLimit,
            Status = account.Status.ToString(),
            CreatedAt = account.CreatedAt
        };
    }

    public async Task<CreateAccountResponse?> GetAccountAsync(Guid accountId)
    {
        var account = await _unitOfWork.Accounts.GetByAccountIdAsync(accountId);
        if (account == null)
            return null;

        return new CreateAccountResponse
        {
            AccountId = account.Id,
            ClientId = account.ClientId,
            Balance = account.Balance,
            ReservedBalance = account.ReservedBalance,
            AvailableBalance = account.AvailableBalance,
            CreditLimit = account.CreditLimit,
            Status = account.Status.ToString(),
            CreatedAt = account.CreatedAt
        };
    }
}

