
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.RateLimit;
using Shared.Constants;

namespace Infrastructure.Resilience
{
    public static class ResiliencePolicies
    {
        // 指数退避重试策略：3次重试，延迟2^attempt秒
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger logger) =>
            Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(msg => !msg.IsSuccessStatusCode)
                .WaitAndRetryAsync(Constants.MAX_RETRY_ATTEMPTS,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        logger.LogWarning($"第{retryCount}次重试，等待{timespan.TotalMilliseconds}ms, 原因:{outcome.Exception?.Message}");
                    });

        // 熔断策略：5次连续失败后熔断30秒，半开后单次探测
        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ILogger logger) =>
            Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: Constants.CIRCUIT_BREAKER_FAILURE_THRESHOLD,
                    durationOfBreak: TimeSpan.FromSeconds(Constants.CIRCUIT_BREAKER_BREAK_DURATION_SECONDS),
                    onBreak: (ex, breakDelay) =>
                    {
                        logger.LogError($"熔断器已打开，暂停{breakDelay.TotalSeconds}s，错误:{ex.Exception?.Message}");
                    },
                    onReset: () => logger.LogInformation("熔断器已恢复，请求恢复正常"),
                    onHalfOpen: () => logger.LogWarning("熔断器半开，探测下游服务状态")
                );

        // 限流 + 舱壁隔离组合策略
        public static IAsyncPolicy<HttpResponseMessage> GetBulkheadPolicy(
            int maxParallelization,
            int maxQueuingActions,
            ILogger logger)
        {
            return Policy.BulkheadAsync<HttpResponseMessage>(
                maxParallelization,
                maxQueuingActions,
                onBulkheadRejectedAsync: (context) =>
                {
                    logger.LogWarning($"请求被舱壁隔离器拒绝，当前并发已达上限 {maxParallelization}");
                    return Task.CompletedTask;
                });
        }
    }
}
