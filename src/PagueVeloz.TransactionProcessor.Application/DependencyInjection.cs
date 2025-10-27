using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PagueVeloz.TransactionProcessor.Application.Services;
using PagueVeloz.TransactionProcessor.Application.Validators;

namespace PagueVeloz.TransactionProcessor.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IAccountValidationService, AccountValidationService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddSingleton<IMetricsService, MetricsService>();

        // Register validators
        services.AddValidatorsFromAssemblyContaining<CreateAccountRequestValidator>();

        return services;
    }
}

