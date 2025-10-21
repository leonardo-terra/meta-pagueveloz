using Microsoft.Extensions.DependencyInjection;
using PagueVeloz.TransactionProcessor.Application.Services;

namespace PagueVeloz.TransactionProcessor.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ITransactionService, TransactionService>();

        return services;
    }
}

