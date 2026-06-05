using Microsoft.Extensions.Logging;
using ML.Service.Services;
using Moq;
using Shared.DTOs;
using Shared.Enums;

namespace UnitTests.AgentServiceTests
{
    public class TechnicalIndicatorServiceTests
    {
        private readonly TechnicalIndicatorService _service;
        private readonly Mock<ILogger<TechnicalIndicatorService>> _loggerMock;

        public TechnicalIndicatorServiceTests()
        {
            _loggerMock = new Mock<ILogger<TechnicalIndicatorService>>();
            _service = new TechnicalIndicatorService(_loggerMock.Object);
        }

        #region CalculateIndicators

        [Fact]
        public void CalculateIndicators_WithNullHistoricalData_ReturnsEmptyIndicator()
        {
            // Arrange
            List<StockData>? historicalData = null;

            // Act
            var result = _service.CalculateIndicators(historicalData!);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0m, result.MovingAverage5);
            Assert.Equal(0m, result.MovingAverage10);
            Assert.Equal(0m, result.MovingAverage20);
            Assert.Equal(0m, result.MovingAverage30);
            Assert.Equal(0m, result.MovingAverage60);
            Assert.Equal(0m, result.MovingAverage120);
            Assert.Equal(0m, result.RSI);
            Assert.Equal(0m, result.Volatility);
            Assert.Null(result.PredictedNextPrice);
            Assert.False(result.IsPriceAnomaly);
            Assert.Null(result.AnomalyAlert);
            Assert.Equal(0, (int)result.RiskCluster);
            Assert.Null(result.RiskClusterDescription);
        }

        [Fact]
        public void CalculateIndicators_WithEmptyHistoricalData_ReturnsEmptyIndicator()
        {
            // Arrange
            var historicalData = new List<StockData>();

            // Act
            var result = _service.CalculateIndicators(historicalData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0m, result.MovingAverage5);
        }

        [Fact]
        public void CalculateIndicators_WithSinglePrice_ReturnsBasicIndicators()
        {
            // Arrange
            var stock = new StockData { ExecutionPrice = 100m, Quantity = 1000 };
            var historicalData = new List<StockData> { stock };

            // Act
            var result = _service.CalculateIndicators(historicalData);

            // Assert
            Assert.Equal(100m, result.MovingAverage5);
            Assert.Equal(100m, result.MovingAverage10);
            Assert.Equal(100m, result.MovingAverage20);
            Assert.Equal(100m, result.MovingAverage30);
            Assert.Equal(100m, result.MovingAverage60);
            Assert.Equal(100m, result.MovingAverage120);
            Assert.Equal(0m, result.RSI);            // 数据不足 RSI 默认为0
            Assert.Equal(0m, result.Volatility);     // 至少2个点才能计算波动率
            Assert.Null(result.PredictedNextPrice);  // 数据不足10个点
            Assert.False(result.IsPriceAnomaly);
            Assert.Equal(0, (int)result.RiskCluster);     // 聚类需要至少5个点
        }

        [Fact]
        public void CalculateIndicators_WithSufficientData_CalculatesAllIndicators()
        {
            // Arrange: 生成120个随机价格点，确保所有移动平均线都有数据
            var historicalData = GenerateRandomStockData(120, startPrice: 100m, volatility: 0.02m);

            // Act
            var result = _service.CalculateIndicators(historicalData);

            // Assert
            // 移动平均线应该计算有效值
            Assert.InRange(result.MovingAverage5, 0m, 200m);
            Assert.InRange(result.MovingAverage10, 0m, 200m);
            Assert.InRange(result.MovingAverage20, 0m, 200m);
            Assert.InRange(result.MovingAverage30, 0m, 200m);
            Assert.InRange(result.MovingAverage60, 0m, 200m);
            Assert.InRange(result.MovingAverage120, 0m, 200m);
            // RSI 应在 0-100 之间
            Assert.InRange(result.RSI, 0m, 100m);
            // 波动率应为正数
            Assert.True(result.Volatility > 0);

            // ML.NET 预测 (需要至少10个点)
            Assert.NotNull(result.PredictedNextPrice);
            Assert.True(result.PredictedNextPrice > 0);

            // 异常检测 (需要至少10个点)
            // 由于随机数据可能没有突变，IsPriceAnomaly 可能为 false，但 alert 不应为空
            Assert.NotNull(result.AnomalyAlert);

            // 聚类 (需要至少5个点)
            Assert.InRange((int)result.RiskCluster + 1, (int)RiskCluster.Low, (int)RiskCluster.High);
            Assert.NotNull(result.RiskClusterDescription);
        }

        #endregion

        #region CalculateVaR

        [Fact]
        public void CalculateVaR_WithNullReturns_ReturnsZero()
        {
            // Act
            var result = _service.CalculateVaR(null!);

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public void CalculateVaR_WithEmptyReturns_ReturnsZero()
        {
            // Act
            var result = _service.CalculateVaR(new List<decimal>());

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public void CalculateVaR_WithNormalReturns_ReturnsCorrectPercentile()
        {
            // Arrange: 收益率序列 [-0.1, -0.05, 0, 0.02, 0.05, 0.08, 0.1]
            var returns = new List<decimal> { -0.10m, -0.05m, 0m, 0.02m, 0.05m, 0.08m, 0.10m };

            // Act: 95% VaR 对应第 (1-0.95)*7 = 0.35 -> 第0个元素 (索引0)
            var result = _service.CalculateVaR(returns, 0.95);

            // Assert: 排序后最小值为 -0.10
            Assert.Equal(-0.10m, result);
        }

        [Fact]
        public void CalculateVaR_WithConfidence99_ReturnsCorrectPercentile()
        {
            // Arrange
            var returns = new List<decimal> { -0.15m, -0.08m, -0.02m, 0.01m, 0.03m, 0.05m, 0.07m };

            // Act: 99% VaR -> index = floor(0.01*7) = 0
            var result = _service.CalculateVaR(returns, 0.99);

            // Assert
            Assert.Equal(-0.15m, result);
        }

        [Fact]
        public void CalculateVaR_WhenConfidenceOutOfRange_ClampsIndex()
        {
            // Arrange
            var returns = new List<decimal> { 0.01m, 0.02m };

            // Act: confidence=0.9999 => index = floor((1-0.9999)*2)=0
            var result = _service.CalculateVaR(returns, 0.9999);

            // Assert: 返回最小值
            Assert.Equal(0.01m, result);
        }

        #endregion

        #region CalculateSharpeRatio

        [Fact]
        public void CalculateSharpeRatio_NullReturns_ReturnsZero()
        {
            // Act
            var result = _service.CalculateSharpeRatio(null!);

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public void CalculateSharpeRatio_EmptyReturns_ReturnsZero()
        {
            // Act
            var result = _service.CalculateSharpeRatio(new List<decimal>());

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public void CalculateSharpeRatio_SingleReturn_ReturnsZero()
        {
            // Act
            var result = _service.CalculateSharpeRatio(new List<decimal> { 0.01m });

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public void CalculateSharpeRatio_ConstantReturns_ReturnsZero()
        {
            // Arrange: 所有收益率相同，标准差为0
            var returns = new List<decimal> { 0.02m, 0.02m, 0.02m, 0.02m, 0.02m };

            // Act
            var result = _service.CalculateSharpeRatio(returns, 0.02m);

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public void CalculateSharpeRatio_WithNormalReturns_ReturnsPositive()
        {
            // Arrange: 月收益率平均为正且有一定波动
            var returns = new List<decimal> { 0.03m, 0.028m, -0.005m, 0.026m, 0.025m, 0.016m, 0.03m };

            // Act
            var result = _service.CalculateSharpeRatio(returns, 0.02m);

            // Assert: 夏普比率应大于0（年化后）
            Assert.True(result > 0);
        }

        #endregion

        #region CalculateMaxDrawdown

        [Fact]
        public void CalculateMaxDrawdown_NullNav_ReturnsZero()
        {
            // Act
            var result = _service.CalculateMaxDrawdown(null!);

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public void CalculateMaxDrawdown_EmptyNav_ReturnsZero()
        {
            // Act
            var result = _service.CalculateMaxDrawdown(new List<decimal>());

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public void CalculateMaxDrawdown_SingleValue_ReturnsZero()
        {
            // Act
            var result = _service.CalculateMaxDrawdown(new List<decimal> { 100m });

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public void CalculateMaxDrawdown_IncreasingPrices_ReturnsZero()
        {
            // Arrange
            var nav = new List<decimal> { 100m, 105m, 110m, 115m, 120m };

            // Act
            var result = _service.CalculateMaxDrawdown(nav);

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public void CalculateMaxDrawdown_DecreasingPrices_ReturnsCorrectDrawdown()
        {
            // Arrange
            var nav = new List<decimal> { 100m, 90m, 80m, 70m };

            // Act
            var result = _service.CalculateMaxDrawdown(nav);

            // Assert: 最大回撤 = (100-70)/100 = 0.3
            Assert.Equal(0.3m, result);
        }

        [Fact]
        public void CalculateMaxDrawdown_WithRecovery_ReturnsMaxFromPeak()
        {
            // Arrange: 100 -> 120 (peak) -> 90 (drawdown=0.25) -> 110 -> 80 (drawdown from 120=0.3333)
            var nav = new List<decimal> { 100m, 120m, 90m, 110m, 80m };

            // Act
            var result = _service.CalculateMaxDrawdown(nav);

            // Assert: (120-80)/120 = 0.3333...
            Assert.Equal(0.3333333333333333m, result, 10);
        }

        #endregion

        #region Helper: Generate test data

        private List<StockData> GenerateRandomStockData(int count, decimal startPrice, decimal volatility)
        {
            var rand = new Random(42);
            var data = new List<StockData>();
            decimal price = startPrice;
            for (int i = 0; i < count; i++)
            {
                // 简单的随机游走
                decimal change = (decimal)((rand.NextDouble() - 0.5) * 2 * (double)volatility);
                price = price * (1 + change);
                if (price <= 0) price = 0.01m;
                data.Add(new StockData
                {
                    ExecutionPrice = price,
                    Quantity = rand.Next(1000, 100000),
                    Date = DateTime.Today.AddDays(-count + i)
                });
            }
            return data;
        }

        #endregion
    }
}
