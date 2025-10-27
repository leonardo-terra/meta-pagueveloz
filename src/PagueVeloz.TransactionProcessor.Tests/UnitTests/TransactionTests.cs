using Xunit;
using PagueVeloz.TransactionProcessor.Domain.Entities;

namespace PagueVeloz.TransactionProcessor.Tests.UnitTests;

public class TransactionTests
{
    [Fact]
    public void Transaction_NewTransaction_ShouldHaveDefaultValues()
    {
        var transaction = new Transaction();

        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.Equal(TransactionStatus.Pending, transaction.Status);
        Assert.True((DateTime.UtcNow - transaction.CreatedAt).TotalSeconds < 1);
        Assert.Null(transaction.ProcessedAt);
        Assert.Null(transaction.ErrorMessage);
    }

    [Fact]
    public void Transaction_MarkAsSuccess_ShouldUpdateStatusAndTimestamp()
    {
        var transaction = new Transaction
        {
            Status = TransactionStatus.Pending
        };

        transaction.MarkAsSuccess();

        Assert.Equal(TransactionStatus.Success, transaction.Status);
        Assert.NotNull(transaction.ProcessedAt);
        Assert.True((DateTime.UtcNow - transaction.ProcessedAt.Value).TotalSeconds < 1);
        Assert.Null(transaction.ErrorMessage);
    }

    [Fact]
    public void Transaction_MarkAsFailed_ShouldUpdateStatusTimestampAndError()
    {
        var transaction = new Transaction
        {
            Status = TransactionStatus.Pending
        };
        var errorMessage = "Insufficient balance";

        transaction.MarkAsFailed(errorMessage);

        Assert.Equal(TransactionStatus.Failed, transaction.Status);
        Assert.NotNull(transaction.ProcessedAt);
        Assert.True((DateTime.UtcNow - transaction.ProcessedAt.Value).TotalSeconds < 1);
        Assert.Equal(errorMessage, transaction.ErrorMessage);
    }

    [Fact]
    public void Transaction_MarkAsFailed_WithNullMessage_ShouldStoreNull()
    {
        var transaction = new Transaction
        {
            Status = TransactionStatus.Pending
        };

        transaction.MarkAsFailed(null);

        Assert.Equal(TransactionStatus.Failed, transaction.Status);
        Assert.NotNull(transaction.ProcessedAt);
        Assert.Null(transaction.ErrorMessage);
    }

    [Fact]
    public void Transaction_MarkAsSuccess_AfterFailed_ShouldUpdateCorrectly()
    {
        var transaction = new Transaction
        {
            Status = TransactionStatus.Failed,
            ErrorMessage = "Previous error"
        };

        transaction.MarkAsSuccess();

        Assert.Equal(TransactionStatus.Success, transaction.Status);
        Assert.NotNull(transaction.ProcessedAt);
        Assert.Null(transaction.ErrorMessage);
    }

    [Theory]
    [InlineData(OperationType.Credit)]
    [InlineData(OperationType.Debit)]
    [InlineData(OperationType.Reserve)]
    [InlineData(OperationType.Capture)]
    [InlineData(OperationType.Reversal)]
    [InlineData(OperationType.Transfer)]
    public void Transaction_Operation_ShouldAcceptAllTypes(OperationType operationType)
    {
        var transaction = new Transaction
        {
            Operation = operationType
        };

        Assert.Equal(operationType, transaction.Operation);
    }

    [Fact]
    public void Transaction_Amount_ShouldStoreCorrectValue()
    {
        var transaction = new Transaction
        {
            Amount = 123.45m
        };

        Assert.Equal(123.45m, transaction.Amount);
    }

    [Fact]
    public void Transaction_Currency_ShouldStoreCorrectValue()
    {
        var transaction = new Transaction
        {
            Currency = "USD"
        };

        Assert.Equal("USD", transaction.Currency);
    }

    [Fact]
    public void Transaction_ReferenceId_ShouldBeUnique()
    {
        var refId = Guid.NewGuid().ToString();
        var transaction = new Transaction
        {
            ReferenceId = refId
        };

        Assert.Equal(refId, transaction.ReferenceId);
    }
}
