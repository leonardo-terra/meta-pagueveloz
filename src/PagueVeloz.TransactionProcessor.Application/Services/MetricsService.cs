using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace PagueVeloz.TransactionProcessor.Application.Services;

public class MetricsService : IMetricsService
{
    private readonly ILogger<MetricsService> _logger;
    private readonly ConcurrentDictionary<string, long> _counters = new();
    private readonly ConcurrentDictionary<string, List<long>> _durations = new();

    public MetricsService(ILogger<MetricsService> logger)
    {
        _logger = logger;
    }

    public void IncrementTransactionCounter(string operation, string status)
    {
        var key = $"transactions_{operation}_{status}";
        var count = _counters.AddOrUpdate(key, 1, (k, v) => v + 1);
        
        _logger.LogInformation("Transaction counter updated: {Operation} {Status} - Total: {Count}",
            operation, status, count);
    }

    public void IncrementConcurrencyConflictCounter()
    {
        var count = _counters.AddOrUpdate("concurrency_conflicts", 1, (k, v) => v + 1);
        
        _logger.LogWarning("Concurrency conflict detected - Total conflicts: {Count}", count);
    }

    public void RecordTransactionDuration(string operation, long durationMs)
    {
        var key = $"duration_{operation}";
        _durations.AddOrUpdate(key, 
            new List<long> { durationMs }, 
            (k, v) => 
            {
                lock (v)
                {
                    v.Add(durationMs);
                    if (v.Count > 1000) v.RemoveAt(0);
                }
                return v;
            });

        _logger.LogInformation("Transaction duration recorded: {Operation} - {Duration}ms", operation, durationMs);
    }

    public void RecordAccountBalance(Guid accountId, decimal balance)
    {
        _logger.LogInformation("Account balance recorded: {AccountId} - {Balance}", accountId, balance);
    }

    public void IncrementValidationFailureCounter(string validationType)
    {
        var key = $"validation_failures_{validationType}";
        var count = _counters.AddOrUpdate(key, 1, (k, v) => v + 1);
        
        _logger.LogWarning("Validation failure: {ValidationType} - Total failures: {Count}",
            validationType, count);
    }

    public Dictionary<string, object> GetMetrics()
    {
        var metrics = new Dictionary<string, object>();
        
        foreach (var counter in _counters)
        {
            metrics[counter.Key] = counter.Value;
        }

        foreach (var duration in _durations)
        {
            lock (duration.Value)
            {
                if (duration.Value.Any())
                {
                    metrics[$"{duration.Key}_avg"] = duration.Value.Average();
                    metrics[$"{duration.Key}_max"] = duration.Value.Max();
                    metrics[$"{duration.Key}_min"] = duration.Value.Min();
                    metrics[$"{duration.Key}_count"] = duration.Value.Count;
                }
            }
        }

        return metrics;
    }
}

