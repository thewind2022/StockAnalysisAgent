using Infrastructure.Resilience;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Polly.CircuitBreaker;
using System.Reflection;

namespace UnitTests.AgentServiceTests
{
    public class CircuitBreakerTests
    {
        [Fact]
        public async Task CircuitBreaker_ShouldBreakAfterFailures()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            // 使用 Protected 模拟受保护的 SendAsync 方法
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Simulated failure"));

            var httpClient = new HttpClient(mockHandler.Object);
            var mockLogger = new Mock<ILogger<Program>>();

            var policy = ResiliencePolicies.GetCircuitBreakerPolicy(mockLogger.Object);

            // 定义一个会触发熔断的委托
            Func<Task> action = async () =>
            {
                await policy.ExecuteAsync(async token =>
                {
                    var response = await httpClient.GetAsync("http://api.example.com", token);
                    response.EnsureSuccessStatusCode();
                    return response;
                }, CancellationToken.None);
            };

            // Act & Assert
            // 前5次调用触发失败（假设断路器配置的失败次数为5）
            for (int i = 0; i < 5; i++)
            {
                await Assert.ThrowsAsync<HttpRequestException>(action);
            }

            // 第6次应触发断路器打开，抛出 BrokenCircuitException
            await Assert.ThrowsAsync<BrokenCircuitException>(action);
        }
    }
}
