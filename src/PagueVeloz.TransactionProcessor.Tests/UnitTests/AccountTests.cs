using Xunit;
using PagueVeloz.TransactionProcessor.Domain.Entities;

namespace PagueVeloz.TransactionProcessor.Tests.UnitTests;

public class AccountTests
{
    [Fact]
    public void Account_AvailableBalance_ShouldCalculateCorrectly()
    {
        var account = new Account
        {
            Balance = 1000m,
            ReservedBalance = 200m,
            CreditLimit = 500m
        };

        Assert.Equal(800m, account.AvailableBalance);
    }

    [Fact]
    public void Account_TotalAvailableBalance_ShouldIncludeCreditLimit()
    {
        var account = new Account
        {
            Balance = 1000m,
            ReservedBalance = 200m,
            CreditLimit = 500m
        };

        Assert.Equal(1300m, account.TotalAvailableBalance);
    }

    [Theory]
    [InlineData(1000, 200, 500, 500, true)]
    [InlineData(1000, 200, 500, 1300, true)]
    [InlineData(1000, 200, 500, 1301, false)]
    [InlineData(1000, 200, 0, 800, true)]
    [InlineData(1000, 200, 0, 801, false)]
    public void Account_CanDebit_ShouldValidateAgainstTotalAvailableBalance(
        decimal balance, decimal reserved, decimal creditLimit, decimal amount, bool expected)
    {
        var account = new Account
        {
            Balance = balance,
            ReservedBalance = reserved,
            CreditLimit = creditLimit
        };

        Assert.Equal(expected, account.CanDebit(amount));
    }

    [Theory]
    [InlineData(1000, 200, 500, 500, true)]
    [InlineData(1000, 200, 500, 800, true)]
    [InlineData(1000, 200, 500, 801, false)]
    [InlineData(1000, 500, 0, 500, true)]
    [InlineData(1000, 500, 0, 501, false)]
    public void Account_CanReserve_ShouldValidateAgainstAvailableBalance(
        decimal balance, decimal reserved, decimal creditLimit, decimal amount, bool expected)
    {
        var account = new Account
        {
            Balance = balance,
            ReservedBalance = reserved,
            CreditLimit = creditLimit
        };

        Assert.Equal(expected, account.CanReserve(amount));
    }

    [Theory]
    [InlineData(500, 200, true)]
    [InlineData(500, 500, true)]
    [InlineData(500, 501, false)]
    [InlineData(0, 1, false)]
    public void Account_CanCapture_ShouldValidateAgainstReservedBalance(
        decimal reserved, decimal amount, bool expected)
    {
        var account = new Account
        {
            ReservedBalance = reserved
        };

        Assert.Equal(expected, account.CanCapture(amount));
    }

    [Fact]
    public void Account_NewAccount_ShouldHaveDefaultValues()
    {
        var account = new Account();

        Assert.NotEqual(Guid.Empty, account.Id);
        Assert.Equal(0m, account.Balance);
        Assert.Equal(0m, account.ReservedBalance);
        Assert.Equal(0m, account.CreditLimit);
        Assert.Equal(AccountStatus.Active, account.Status);
        Assert.True((DateTime.UtcNow - account.CreatedAt).TotalSeconds < 1);
        Assert.Null(account.UpdatedAt);
    }

    [Fact]
    public void Account_AvailableBalance_WithNegativeReserved_ShouldCalculateCorrectly()
    {
        var account = new Account
        {
            Balance = 1000m,
            ReservedBalance = 0m,
            CreditLimit = 0m
        };

        Assert.Equal(1000m, account.AvailableBalance);
    }

    [Fact]
    public void Account_TotalAvailableBalance_WithZeroCreditLimit_ShouldEqualAvailableBalance()
    {
        var account = new Account
        {
            Balance = 1000m,
            ReservedBalance = 200m,
            CreditLimit = 0m
        };

        Assert.Equal(account.AvailableBalance, account.TotalAvailableBalance);
    }

    [Theory]
    [InlineData(0, 0, 0, 0.01, false)]
    [InlineData(100, 0, 0, 100, true)]
    [InlineData(100, 0, 0, 100.01, false)]
    public void Account_CanDebit_EdgeCases_ShouldValidateCorrectly(
        decimal balance, decimal reserved, decimal creditLimit, decimal amount, bool expected)
    {
        var account = new Account
        {
            Balance = balance,
            ReservedBalance = reserved,
            CreditLimit = creditLimit
        };

        Assert.Equal(expected, account.CanDebit(amount));
    }
}
