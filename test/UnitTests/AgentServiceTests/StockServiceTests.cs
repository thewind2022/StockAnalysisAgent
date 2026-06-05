
using AgentService.Services;
using Infrastructure.Resilience;
using Infrastructure.StockRepository;
using Microsoft.Extensions.Logging;
using Moq;
using Polly.CircuitBreaker;
using Shared.DTOs;
using Shared.Services;
using System.Reflection;


namespace UnitTests.AgentServiceTests
{

    public class StockServiceTests
    {
        private readonly Mock<IStockRepository> _stockRepoMock;
        private readonly Mock<IOnnxPredictionService> _onnxServiceMock;
        private readonly Mock<ITechnicalIndicatorService> _indicatorServiceMock;
        private readonly Mock<ILogger<StockService>> _loggerMock;
        private readonly StockService _stockService;

        public StockServiceTests()
        {
            _stockRepoMock = new Mock<IStockRepository>();
            _onnxServiceMock = new Mock<IOnnxPredictionService>();
            _indicatorServiceMock = new Mock<ITechnicalIndicatorService>();
            _loggerMock = new Mock<ILogger<StockService>>();
            _stockService = new StockService(
                _stockRepoMock.Object,
                _onnxServiceMock.Object,
                _indicatorServiceMock.Object,
                _loggerMock.Object);
        }

        #region GetStockAsync

        [Fact]
        public async Task GetStockAsync_WhenStockExists_ReturnsStockData()
        {
            // Arrange
            var expectedStock = new StockData { SecurityCode = "600036", SecurityDesc = "招商银行", ExecutionPrice = 33.20m };
            _stockRepoMock.Setup(repo => repo.GetStockByCodeAsync("600036", It.IsAny<DateTime>()))
                          .ReturnsAsync(expectedStock);

            // Act
            var result = await _stockService.GetStockAsync("600036", DateTime.Today);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("600036", result.SecurityCode);
            _stockRepoMock.Verify(repo => repo.GetStockByCodeAsync("600036", It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task GetStockAsync_WhenStockDoesNotExist_ReturnsNull()
        {
            // Arrange
            _stockRepoMock.Setup(repo => repo.GetStockByCodeAsync("INVALID", It.IsAny<DateTime>()))
                          .ReturnsAsync((StockData?)null);

            // Act
            var result = await _stockService.GetStockAsync("INVALID", DateTime.Today);

            // Assert
            Assert.Null(result);
            _stockRepoMock.Verify(repo => repo.GetStockByCodeAsync("INVALID", It.IsAny<DateTime>()), Times.Once);
        }

        #endregion

        #region CalculatePnlAsync

        [Theory]
        [InlineData(1000, 10.0, 15.0, 5000)]    // 盈利
        [InlineData(1000, 10.0, 5.0, -5000)]     // 亏损
        [InlineData(1000, 10.0, 10.0, 0)]        // 持平
        public async Task CalculatePnlAsync_ReturnsCorrectPnL(long quantity, decimal avgCost, decimal currentPrice, decimal expectedPnl)
        {
            // Act
            var pnl = await _stockService.CalculatePnlAsync(quantity, avgCost, currentPrice);

            // Assert
            Assert.Equal(expectedPnl, pnl);
        }

        #endregion

        #region CalculateRiskMetricsAsync
        [Fact]
        public async Task CalculateRiskMetricsAsync_WithAtLeastFiveElements_ReturnsRiskMetrics()
        {
            // Arrange
            var nav = new List<decimal> { 100m, 110m, 105m, 120m, 115m }; // 5 elements
            var expectedMaxDrawdown = 0.1m;
            var expectedSharpeRatio = 1.2m;
            var expectedVaR95 = -0.03m;

            _indicatorServiceMock.Setup(x => x.CalculateMaxDrawdown(nav)).Returns(expectedMaxDrawdown);
            _indicatorServiceMock.Setup(x => x.CalculateSharpeRatio(It.IsAny<List<decimal>>())).Returns(expectedSharpeRatio);
            _indicatorServiceMock.Setup(x => x.CalculateVaR(It.IsAny<List<decimal>>(), 0.95)).Returns(expectedVaR95);

            // Act
            var result = await _stockService.CalculateRiskMetricsAsync(nav);

            // Assert
            Assert.Equal(expectedMaxDrawdown, result.MaxDrawdown);
            Assert.Equal(expectedSharpeRatio, result.SharpeRatio);
            Assert.Equal(expectedVaR95, result.VaR95);

            // 验证 CalculateMaxDrawdown 使用了正确的 nav 列表
            _indicatorServiceMock.Verify(x => x.CalculateMaxDrawdown(nav), Times.Once);
            // 验证 CalculateSharpeRatio 接收到了长度为 4 的收益率列表（由 5 个净值点计算得出）
            _indicatorServiceMock.Verify(x => x.CalculateSharpeRatio(It.Is<List<decimal>>(r => r.Count == 4)), Times.Once);
            // 验证 CalculateVaR 接收到了同样的收益率列表
            _indicatorServiceMock.Verify(x => x.CalculateVaR(It.Is<List<decimal>>(r => r.Count == 4), 0.95), Times.Once);
        }

        [Fact]
        public async Task CalculateRiskMetricsAsync_WhenHistoricalNavHasLessThanTwoPoints_ReturnsEmptyRiskMetrics()
        {
            // Arrange
            var nav = new List<decimal> { 100m };

            // Act
            var result = await _stockService.CalculateRiskMetricsAsync(nav);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0m, result.MaxDrawdown);
            Assert.Equal(0m, result.SharpeRatio);
            Assert.Equal(0m, result.VaR95);
            _indicatorServiceMock.Verify(x => x.CalculateMaxDrawdown(It.IsAny<List<decimal>>()), Times.Never);
        }

        [Fact]
        public async Task CalculateRiskMetricsAsync_WithValidData_CalculatesAndReturnsRiskMetrics()
        {
            // Arrange
            var nav = new List<decimal> { 100m, 110m, 105m, 120m, 115m };
            var expectedReturns = new List<decimal> { 0.1m, -0.0454545m, 0.142857m, -0.0416667m };
            var expectedMaxDrawdown = 0.1m;
            var expectedSharpeRatio = 1.2m;
            var expectedVaR95 = -0.03m;

            _indicatorServiceMock.Setup(x => x.CalculateMaxDrawdown(nav)).Returns(expectedMaxDrawdown);
            _indicatorServiceMock.Setup(x => x.CalculateSharpeRatio(It.IsAny<List<decimal>>())).Returns(expectedSharpeRatio);
            _indicatorServiceMock.Setup(x => x.CalculateVaR(It.IsAny<List<decimal>>(), 0.95)).Returns(expectedVaR95);

            // Act
            var result = await _stockService.CalculateRiskMetricsAsync(nav);

            // Assert
            Assert.Equal(expectedMaxDrawdown, result.MaxDrawdown);
            Assert.Equal(expectedSharpeRatio, result.SharpeRatio);
            Assert.Equal(expectedVaR95, result.VaR95);

            // Verify that CalculateSharpeRatio and CalculateVaR received the correct returns
            _indicatorServiceMock.Verify(x => x.CalculateSharpeRatio(It.Is<List<decimal>>(r => r.Count == 4 && Math.Abs(r[0] - 0.1m) < 0.0001m)), Times.Once);
            _indicatorServiceMock.Verify(x => x.CalculateVaR(It.IsAny<List<decimal>>(), 0.95), Times.Once);
        }

        #endregion

        #region EvaluateRiskUsingOnnxAsync

        [Fact]
        public async Task EvaluateRiskUsingOnnxAsync_WhenTextIsNullOrWhiteSpace_ReturnsDefaultScore()
        {
            // Act
            var result1 = await _stockService.EvaluateRiskUsingOnnxAsync("600036", null!);
            var result2 = await _stockService.EvaluateRiskUsingOnnxAsync("600036", "");
            var result3 = await _stockService.EvaluateRiskUsingOnnxAsync("600036", "   ");

            // Assert
            Assert.Equal(0.5f, result1);
            Assert.Equal(0.5f, result2);
            Assert.Equal(0.5f, result3);
            _onnxServiceMock.Verify(x => x.PredictAsync(It.IsAny<long[]>()), Times.Never);
        }

        [Fact]
        public async Task EvaluateRiskUsingOnnxAsync_WhenOnnxReturnsValidScore_ReturnsClampedScore()
        {
            // Arrange
            var expectedRawScore = 0.85f;
            _onnxServiceMock.Setup(x => x.PredictAsync(It.IsAny<long[]>()))
                            .ReturnsAsync(new float[] { expectedRawScore });

            // Act
            var result = await _stockService.EvaluateRiskUsingOnnxAsync("600036", "Positive news about the stock");

            // Assert
            Assert.Equal(expectedRawScore, result);
            _onnxServiceMock.Verify(x => x.PredictAsync(It.Is<long[]>(arr => arr.Length == 5 && arr[0] == 101)), Times.Once);
        }

        [Fact]
        public async Task EvaluateRiskUsingOnnxAsync_WhenOnnxReturnsScoreOutOfRange_ClampsTo01()
        {
            // Arrange
            _onnxServiceMock.Setup(x => x.PredictAsync(It.IsAny<long[]>()))
                            .ReturnsAsync(new float[] { 1.5f });

            // Act
            var result = await _stockService.EvaluateRiskUsingOnnxAsync("600036", "test");

            // Assert
            Assert.Equal(1.0f, result); // 应为 clamp 到 1

            // 测试负值
            _onnxServiceMock.Setup(x => x.PredictAsync(It.IsAny<long[]>()))
                            .ReturnsAsync(new float[] { -0.2f });
            result = await _stockService.EvaluateRiskUsingOnnxAsync("600036", "test");
            Assert.Equal(0.0f, result);
        }

        [Fact]
        public async Task EvaluateRiskUsingOnnxAsync_WhenOnnxReturnsEmptyArray_ReturnsDefaultScore()
        {
            // Arrange
            _onnxServiceMock.Setup(x => x.PredictAsync(It.IsAny<long[]>()))
                            .ReturnsAsync(Array.Empty<float>());

            // Act
            var result = await _stockService.EvaluateRiskUsingOnnxAsync("600036", "test");

            // Assert
            Assert.Equal(0.5f, result);
        }

        [Fact]
        public async Task EvaluateRiskUsingOnnxAsync_WhenOnnxThrowsException_ReturnsDefaultScore()
        {
            // Arrange
            _onnxServiceMock.Setup(x => x.PredictAsync(It.IsAny<long[]>()))
                            .ThrowsAsync(new Exception("ONNX runtime error"));

            // Act
            var result = await _stockService.EvaluateRiskUsingOnnxAsync("600036", "test");

            // Assert
            Assert.Equal(0.5f, result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ONNX 推理失败")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion
    }
}
