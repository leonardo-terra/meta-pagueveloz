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

