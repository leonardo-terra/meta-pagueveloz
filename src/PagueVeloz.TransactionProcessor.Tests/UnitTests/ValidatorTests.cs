using Xunit;
using FluentValidation.TestHelper;
using PagueVeloz.TransactionProcessor.Application.DTOs;
using PagueVeloz.TransactionProcessor.Application.Validators;
using PagueVeloz.TransactionProcessor.Domain.Entities;

namespace PagueVeloz.TransactionProcessor.Tests.UnitTests;

public class ValidatorTests
{
    [Fact]
    public void CreateAccountRequestValidator_ValidRequest_ShouldNotHaveErrors()
    {
        var validator = new CreateAccountRequestValidator();
        var request = new CreateAccountRequest
        {
            ClientId = Guid.NewGuid(),
            InitialBalance = 500m,
            CreditLimit = 1000m
        };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateAccountRequestValidator_EmptyClientId_ShouldHaveError()
    {
        var validator = new CreateAccountRequestValidator();
        var request = new CreateAccountRequest
        {
            ClientId = Guid.Empty,
            InitialBalance = 1000m,
            CreditLimit = 500m
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.ClientId);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void CreateAccountRequestValidator_NegativeInitialBalance_ShouldHaveError(decimal balance)
    {
        var validator = new CreateAccountRequestValidator();
        var request = new CreateAccountRequest
        {
            ClientId = Guid.NewGuid(),
            InitialBalance = balance,
            CreditLimit = 0m
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.InitialBalance);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-500)]
    public void CreateAccountRequestValidator_NegativeCreditLimit_ShouldHaveError(decimal creditLimit)
    {
        var validator = new CreateAccountRequestValidator();
        var request = new CreateAccountRequest
        {
            ClientId = Guid.NewGuid(),
            InitialBalance = 0m,
            CreditLimit = creditLimit
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.CreditLimit);
    }

    [Fact]
    public void ProcessTransactionRequestValidator_ValidRequest_ShouldNotHaveErrors()
    {
        var validator = new ProcessTransactionRequestValidator();
        var request = new ProcessTransactionRequest
        {
            AccountId = Guid.NewGuid(),
            ReferenceId = Guid.NewGuid().ToString(),
            Operation = OperationType.Credit,
            Amount = 100m,
            Currency = "BRL"
        };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ProcessTransactionRequestValidator_EmptyAccountId_ShouldHaveError()
    {
        var validator = new ProcessTransactionRequestValidator();
        var request = new ProcessTransactionRequest
        {
            AccountId = Guid.Empty,
            ReferenceId = Guid.NewGuid().ToString(),
            Operation = OperationType.Credit,
            Amount = 100m,
            Currency = "BRL"
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.AccountId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ProcessTransactionRequestValidator_InvalidReferenceId_ShouldHaveError(string referenceId)
    {
        var validator = new ProcessTransactionRequestValidator();
        var request = new ProcessTransactionRequest
        {
            AccountId = Guid.NewGuid(),
            ReferenceId = referenceId,
            Operation = OperationType.Credit,
            Amount = 100m,
            Currency = "BRL"
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.ReferenceId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void ProcessTransactionRequestValidator_InvalidAmount_ShouldHaveError(decimal amount)
    {
        var validator = new ProcessTransactionRequestValidator();
        var request = new ProcessTransactionRequest
        {
            AccountId = Guid.NewGuid(),
            ReferenceId = Guid.NewGuid().ToString(),
            Operation = OperationType.Credit,
            Amount = amount,
            Currency = "BRL"
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Amount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ProcessTransactionRequestValidator_InvalidCurrency_ShouldHaveError(string currency)
    {
        var validator = new ProcessTransactionRequestValidator();
        var request = new ProcessTransactionRequest
        {
            AccountId = Guid.NewGuid(),
            ReferenceId = Guid.NewGuid().ToString(),
            Operation = OperationType.Credit,
            Amount = 100m,
            Currency = currency
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Currency);
    }


    [Theory]
    [InlineData("AB")]
    [InlineData("TOOLONG")]
    public void ProcessTransactionRequestValidator_InvalidCurrencyLength_ShouldHaveError(string currency)
    {
        var validator = new ProcessTransactionRequestValidator();
        var request = new ProcessTransactionRequest
        {
            AccountId = Guid.NewGuid(),
            ReferenceId = Guid.NewGuid().ToString(),
            Operation = OperationType.Credit,
            Amount = 100m,
            Currency = currency
        };

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(r => r.Currency);
    }

    [Theory]
    [InlineData("BRL")]
    [InlineData("USD")]
    [InlineData("EUR")]
    public void ProcessTransactionRequestValidator_ValidCurrency_ShouldNotHaveError(string currency)
    {
        var validator = new ProcessTransactionRequestValidator();
        var request = new ProcessTransactionRequest
        {
            AccountId = Guid.NewGuid(),
            ReferenceId = Guid.NewGuid().ToString(),
            Operation = OperationType.Credit,
            Amount = 100m,
            Currency = currency
        };

        var result = validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(r => r.Currency);
    }
}
