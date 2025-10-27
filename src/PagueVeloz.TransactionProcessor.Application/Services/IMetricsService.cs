namespace PagueVeloz.TransactionProcessor.Application.Services;

public interface IMetricsService
{
    void IncrementTransactionCounter(string operation, string status);
    void IncrementConcurrencyConflictCounter();
    void RecordTransactionDuration(string operation, long durationMs);
    void RecordAccountBalance(Guid accountId, decimal balance);
    void IncrementValidationFailureCounter(string validationType);
    Dictionary<string, object> GetMetrics();
}
