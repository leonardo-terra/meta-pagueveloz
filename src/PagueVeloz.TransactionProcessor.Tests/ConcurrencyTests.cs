using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PagueVeloz.TransactionProcessor.Application.DTOs;
using PagueVeloz.TransactionProcessor.Application.Services;
using PagueVeloz.TransactionProcessor.Domain.Entities;
using PagueVeloz.TransactionProcessor.Domain.Interfaces;
using PagueVeloz.TransactionProcessor.Infrastructure.Data;
using PagueVeloz.TransactionProcessor.Infrastructure.Repositories;

namespace PagueVeloz.TransactionProcessor.Tests;

public class ConcurrencyTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly TransactionProcessorDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransactionService _transactionService;
    private readonly IAccountService _accountService;
    private readonly Guid _testAccountId;
    private readonly Guid _testClientId;

    public ConcurrencyTests()
    {
        // Setup in-memory database
        var services = new ServiceCollection();
        services.AddDbContext<TransactionProcessorDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IAccountValidationService, AccountValidationService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddSingleton<IMetricsService, MetricsService>();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<TransactionProcessorDbContext>();
        _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
        _transactionService = _serviceProvider.GetRequiredService<ITransactionService>();
        _accountService = _serviceProvider.GetRequiredService<IAccountService>();

        // Create test data
        _testClientId = Guid.NewGuid();
        _testAccountId = Guid.NewGuid();

        var client = new Client
        {
            Id = _testClientId,
            Name = "Test Client",
            Email = "test@example.com",
            Status = ClientStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var account = new Account
        {
            Id = _testAccountId,
            ClientId = _testClientId,
            Balance = 100000, // R$ 1.000,00
            ReservedBalance = 0,
            CreditLimit = 50000, // R$ 500,00
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _context.Clients.Add(client);
        _context.Accounts.Add(account);
        _context.SaveChanges();
    }

    [Fact]
    public async Task ConcurrentDebitTransactions_ShouldMaintainDataIntegrity()
    {
        // Arrange
        const int numberOfConcurrentTransactions = 100;
        const decimal debitAmount = 1000; // R$ 10,00 each
        var tasks = new List<Task<ProcessTransactionResponse>>();

        // Act - Create 100 concurrent debit transactions
        for (int i = 0; i < numberOfConcurrentTransactions; i++)
        {
            var request = new ProcessTransactionRequest
            {
                Operation = OperationType.Debit,
                AccountId = _testAccountId,
                Amount = debitAmount,
                Currency = "BRL",
                ReferenceId = $"TXN-CONCURRENT-{i:D3}"
            };

            tasks.Add(_transactionService.ProcessTransactionAsync(request));
        }

        // Wait for all transactions to complete
        var results = await Task.WhenAll(tasks);

        // Assert
        var successfulTransactions = results.Count(r => r.Status == "success");
        var failedTransactions = results.Count(r => r.Status == "failed");
        
        // Verify that the total number of transactions equals the expected count
        Assert.Equal(numberOfConcurrentTransactions, results.Length);
        
        // Verify that successful + failed transactions equals total
        Assert.Equal(numberOfConcurrentTransactions, successfulTransactions + failedTransactions);

        // Verify final balance is correct
        var finalAccount = await _unitOfWork.Accounts.GetByAccountIdAsync(_testAccountId);
        Assert.NotNull(finalAccount);
        
        // Expected final balance: 100000 - (successfulTransactions * 1000)
        var expectedFinalBalance = 100000 - (successfulTransactions * debitAmount);
        Assert.Equal(expectedFinalBalance, finalAccount.Balance);

        // Verify that no transaction exceeded the available balance
        // Available balance = Balance + CreditLimit = 100000 + 50000 = 150000
        // Maximum possible debits = 150000 / 1000 = 150
        // Since we only did 100 debits, all should succeed
        Assert.Equal(numberOfConcurrentTransactions, successfulTransactions);
        Assert.Equal(0, failedTransactions);

        // Verify all transactions were recorded
        var allTransactions = await _unitOfWork.Transactions.GetByAccountIdAsync(_testAccountId);
        Assert.Equal(numberOfConcurrentTransactions, allTransactions.Count());
    }

    [Fact]
    public async Task ConcurrentCreditTransactions_ShouldAllSucceed()
    {
        // Arrange
        const int numberOfConcurrentTransactions = 50;
        const decimal creditAmount = 500; // R$ 5,00 each
        var tasks = new List<Task<ProcessTransactionResponse>>();

        // Act - Create 50 concurrent credit transactions
        for (int i = 0; i < numberOfConcurrentTransactions; i++)
        {
            var request = new ProcessTransactionRequest
            {
                Operation = OperationType.Credit,
                AccountId = _testAccountId,
                Amount = creditAmount,
                Currency = "BRL",
                ReferenceId = $"TXN-CREDIT-{i:D3}"
            };

            tasks.Add(_transactionService.ProcessTransactionAsync(request));
        }

        // Wait for all transactions to complete
        var results = await Task.WhenAll(tasks);

        // Assert
        var successfulTransactions = results.Count(r => r.Status == "success");
        var failedTransactions = results.Count(r => r.Status == "failed");
        
        // All credit transactions should succeed
        Assert.Equal(numberOfConcurrentTransactions, successfulTransactions);
        Assert.Equal(0, failedTransactions);

        // Verify final balance is correct
        var finalAccount = await _unitOfWork.Accounts.GetByAccountIdAsync(_testAccountId);
        Assert.NotNull(finalAccount);
        
        // Expected final balance: 100000 + (50 * 500) = 125000
        var expectedFinalBalance = 100000 + (numberOfConcurrentTransactions * creditAmount);
        Assert.Equal(expectedFinalBalance, finalAccount.Balance);
    }

    [Fact]
    public async Task ConcurrentReserveAndCapture_ShouldMaintainDataIntegrity()
    {
        // Arrange
        const int numberOfReserves = 20;
        const decimal reserveAmount = 1000; // R$ 10,00 each
        var reserveTasks = new List<Task<ProcessTransactionResponse>>();
        var captureTasks = new List<Task<ProcessTransactionResponse>>();

        // Act - Create concurrent reserve transactions
        for (int i = 0; i < numberOfReserves; i++)
        {
            var reserveRequest = new ProcessTransactionRequest
            {
                Operation = OperationType.Reserve,
                AccountId = _testAccountId,
                Amount = reserveAmount,
                Currency = "BRL",
                ReferenceId = $"TXN-RESERVE-{i:D3}"
            };

            reserveTasks.Add(_transactionService.ProcessTransactionAsync(reserveRequest));
        }

        // Wait for reserves to complete
        var reserveResults = await Task.WhenAll(reserveTasks);

        // Create concurrent capture transactions
        for (int i = 0; i < numberOfReserves; i++)
        {
            var captureRequest = new ProcessTransactionRequest
            {
                Operation = OperationType.Capture,
                AccountId = _testAccountId,
                Amount = reserveAmount,
                Currency = "BRL",
                ReferenceId = $"TXN-CAPTURE-{i:D3}"
            };

            captureTasks.Add(_transactionService.ProcessTransactionAsync(captureRequest));
        }

        // Wait for captures to complete
        var captureResults = await Task.WhenAll(captureTasks);

        // Assert
        var successfulReserves = reserveResults.Count(r => r.Status == "success");
        var successfulCaptures = captureResults.Count(r => r.Status == "success");

        // All reserves should succeed (we have enough balance)
        Assert.Equal(numberOfReserves, successfulReserves);

        // All captures should succeed (we have enough reserved balance)
        Assert.Equal(numberOfReserves, successfulCaptures);

        // Verify final state
        var finalAccount = await _unitOfWork.Accounts.GetByAccountIdAsync(_testAccountId);
        Assert.NotNull(finalAccount);
        
        // After reserve + capture: balance should decrease by reserveAmount * numberOfReserves
        // Reserved balance should be 0 (all captured)
        var expectedFinalBalance = 100000 - (numberOfReserves * reserveAmount);
        Assert.Equal(expectedFinalBalance, finalAccount.Balance);
        Assert.Equal(0, finalAccount.ReservedBalance);
    }

    [Fact]
    public async Task Idempotency_ShouldPreventDuplicateTransactions()
    {
        // Arrange
        var request = new ProcessTransactionRequest
        {
            Operation = OperationType.Debit,
            AccountId = _testAccountId,
            Amount = 1000,
            Currency = "BRL",
            ReferenceId = "TXN-IDEMPOTENCY-TEST"
        };

        // Act - Execute the same transaction twice
        var result1 = await _transactionService.ProcessTransactionAsync(request);
        var result2 = await _transactionService.ProcessTransactionAsync(request);

        // Assert
        // First transaction should succeed
        Assert.Equal("success", result1.Status);
        Assert.NotEqual(Guid.Empty, result1.TransactionId);

        // Second transaction should return the same result (idempotency)
        Assert.Equal("success", result2.Status);
        Assert.Equal(result1.TransactionId, result2.TransactionId);
        Assert.Equal(result1.Balance, result2.Balance);

        // Verify only one transaction was actually created
        var transactions = await _unitOfWork.Transactions.GetByAccountIdAsync(_testAccountId);
        var idempotencyTransactions = transactions.Where(t => t.ReferenceId == "TXN-IDEMPOTENCY-TEST");
        Assert.Single(idempotencyTransactions);
    }

    [Fact]
    public async Task ConcurrentTransactionsExceedingBalance_ShouldFailAppropriately()
    {
        // Arrange
        const int numberOfConcurrentTransactions = 200; // More than available balance
        const decimal debitAmount = 1000; // R$ 10,00 each
        var tasks = new List<Task<ProcessTransactionResponse>>();

        // Act - Create 200 concurrent debit transactions (total: 200,000)
        // Available balance: 100,000 + 50,000 (credit limit) = 150,000
        // So 150 should succeed, 50 should fail
        for (int i = 0; i < numberOfConcurrentTransactions; i++)
        {
            var request = new ProcessTransactionRequest
            {
                Operation = OperationType.Debit,
                AccountId = _testAccountId,
                Amount = debitAmount,
                Currency = "BRL",
                ReferenceId = $"TXN-EXCEED-{i:D3}"
            };

            tasks.Add(_transactionService.ProcessTransactionAsync(request));
        }

        // Wait for all transactions to complete
        var results = await Task.WhenAll(tasks);

        // Assert
        var successfulTransactions = results.Count(r => r.Status == "success");
        var failedTransactions = results.Count(r => r.Status == "failed");
        
        // Verify that successful + failed transactions equals total
        Assert.Equal(numberOfConcurrentTransactions, successfulTransactions + failedTransactions);

        // Verify that some transactions failed due to insufficient balance
        Assert.True(failedTransactions > 0, "Some transactions should have failed due to insufficient balance");

        // Verify final balance is correct
        var finalAccount = await _unitOfWork.Accounts.GetByAccountIdAsync(_testAccountId);
        Assert.NotNull(finalAccount);
        
        // Final balance should be: 100000 - (successfulTransactions * 1000)
        var expectedFinalBalance = 100000 - (successfulTransactions * debitAmount);
        Assert.Equal(expectedFinalBalance, finalAccount.Balance);

        // Verify that the final balance is not negative
        Assert.True(finalAccount.Balance >= -finalAccount.CreditLimit, "Final balance should not exceed credit limit");
    }

    public void Dispose()
    {
        _context?.Dispose();
        _serviceProvider?.Dispose();
    }
}
