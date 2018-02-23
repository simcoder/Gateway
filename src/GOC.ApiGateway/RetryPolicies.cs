using System;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace GOC.ApiGateway
{
    public class RetryPolicies
    {
        private readonly CircuitBreakerSettings _circuitBreakerSettings;
        private readonly WaitAndRetrySettings _waitAndRetrySettings;
        private readonly ILogger _logger;
        public readonly Policy InventoryServiceCircuitBreaker;
        public readonly Policy CrmServiceCircuitBreaker;


        public RetryPolicies(CircuitBreakerSettings circuitBreakerSettings, WaitAndRetrySettings waitAndRetrySettings, ILoggerFactory loggerFactory)
        {
            _circuitBreakerSettings = circuitBreakerSettings;
            _waitAndRetrySettings = waitAndRetrySettings;
            _logger = loggerFactory.CreateLogger<RetryPolicies>();

            var inventoryCircuitBreaker = CircuitBreakerPolicy();
            var crmCircuitBreaker = CircuitBreakerPolicy();

            var waitAndRetry = WaitAndRetryPolicy();

            CrmServiceCircuitBreaker = Policy.WrapAsync(waitAndRetry, crmCircuitBreaker);
            InventoryServiceCircuitBreaker = Policy.WrapAsync(waitAndRetry, inventoryCircuitBreaker);
        }
        
        private CircuitBreakerPolicy CircuitBreakerPolicy()
        {
            return Policy
                .Handle<Exception>()
                .AdvancedCircuitBreakerAsync(
                    failureThreshold: _circuitBreakerSettings.FailureThreshold,
                    samplingDuration: TimeSpan.FromSeconds(_circuitBreakerSettings.SamplingDurationInSeconds),
                    minimumThroughput: _circuitBreakerSettings.MinimumThroughput,
                    durationOfBreak: TimeSpan.FromSeconds(_circuitBreakerSettings.DurationOfBreakInSeconds),
                    onBreak: (ex, breakDelay) =>
                    {
                        _logger.LogError("Circuit Breaker: Breaking the circuit for " + breakDelay.TotalMilliseconds + "ms!  Exception: {@ex}", ex);
                    },
                    onReset: () => _logger.LogInformation("Circuit Breaker: : Call ok! Closed the circuit again!"),
                    onHalfOpen: () => _logger.LogWarning("Circuit Breaker: Half-open: Next call is a trial!")
                );
        }

        private RetryPolicy WaitAndRetryPolicy()
        {
            return Policy
                .Handle<Exception>(e => !(e is BrokenCircuitException)) // Exception filtering!  We don't retry if the inner circuit-breaker judges the underlying system is out of commission!
                .WaitAndRetryAsync(_waitAndRetrySettings.RetryAttempts,
                    attempt => TimeSpan.FromSeconds(0.1 * Math.Pow(2, attempt)), // Back off!  200ms, 400ms, 800ms, 1600ms
                    (ex, calculatedWaitDuration) =>
                    {
                        _logger.LogError("WaitAndRetry: Delaying for " + calculatedWaitDuration.TotalMilliseconds + "ms. Exception Message: {ExceptionMessage}", ex.Message);
                    });
        }
    }
}
