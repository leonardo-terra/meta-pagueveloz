using PagueVeloz.TransactionProcessor.Application.DTOs;

namespace PagueVeloz.TransactionProcessor.Application.Services;

public interface IAccountService
{
    Task<CreateAccountResponse> CreateAccountAsync(CreateAccountRequest request);
    Task<CreateAccountResponse?> GetAccountAsync(Guid accountId);
}

