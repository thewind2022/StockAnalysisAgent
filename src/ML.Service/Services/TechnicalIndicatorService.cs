
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using Shared.DTOs;
using Shared.Enums;
using Shared.Services;

namespace ML.Service.Services
{
    public class TechnicalIndicatorService : ITechnicalIndicatorService
    {
        #region 内部辅助类

        // 回归预测
        private class PricePredictionInput
        {
            public float PriceMinus5 { get; set; }
            public float PriceMinus4 { get; set; }
            public float PriceMinus3 { get; set; }
            public float PriceMinus2 { get; set; }
            public float PriceMinus1 { get; set; }
            public float Volume { get; set; }
            public float NextPrice { get; set; }
        }

        private class PricePredictionOutput
        {
            [ColumnName("Score")]
            public float PredictedPrice { get; set; }
        }

        // 异常检测
        private class PriceData
        {
            public float Price { get; set; }
        }

        private class ChangePointOutput
        {
            [VectorType(4)]
            public double[]? Prediction { get; set; }
        }

        // 聚类
        private class RiskFeatures
        {
            [VectorType(4)]
            public float[]? Features { get; set; }
        }

        private class ClusterPrediction
        {
            [ColumnName("PredictedLabel")]
            public uint PredictedLabel { get; set; }
        }

        #endregion

        private readonly MLContext _mlContext;
        private readonly ILogger<TechnicalIndicatorService>? _logger;

        public TechnicalIndicatorService(ILogger<TechnicalIndicatorService>? logger = null)
        {
            _mlContext = new MLContext(seed: 42);
            _logger = logger;
        }

        public TechnicalIndicator CalculateIndicators(List<StockData> historicalData)
        {
            if (historicalData == null || historicalData.Count == 0)
                return new TechnicalIndicator();

            var prices = historicalData.Select(d => d.ExecutionPrice).ToList();
            var indicators = new TechnicalIndicator
            {
                MovingAverage5 = CalculateSimpleMovingAverage(prices, MovingAverageType.MA5),
                MovingAverage10 = CalculateSimpleMovingAverage(prices, MovingAverageType.MA10),
                MovingAverage20 = CalculateSimpleMovingAverage(prices, MovingAverageType.MA20),
                MovingAverage30 = CalculateSimpleMovingAverage(prices, MovingAverageType.MA30),
                MovingAverage60 = CalculateSimpleMovingAverage(prices, MovingAverageType.MA60),
                MovingAverage120 = CalculateSimpleMovingAverage(prices, MovingAverageType.MA120),

                RSI = CalculateRSI(prices, 14),
                Volatility = CalculateVolatility(prices)
            };

            // 1. ML.NET 回归预测
            if (historicalData.Count >= 10)
                indicators.PredictedNextPrice = PredictNextPrice(historicalData);

            // 2. ML.NET 异常检测（IID 更改点）
            if (historicalData.Count >= 10)
            {
                var (isAnomaly, alert) = DetectPriceChangePoint(historicalData);
                indicators.IsPriceAnomaly = isAnomaly;
                indicators.AnomalyAlert = alert;
            }

            // 3. ML.NET 聚类风险分类
            if (historicalData.Count >= 5)
            {
                var (cluster, desc) = ClassifyRiskCluster(historicalData);
                indicators.RiskCluster = cluster;
                indicators.RiskClusterDescription = desc;
            }

            return indicators;
        }

        public decimal CalculateVaR(List<decimal> returns, double confidence = 0.95)
        {
            if (returns == null || returns.Count == 0) 
                return 0m;

            var sorted = returns.OrderBy(r => r).ToList();
            int index = (int)((1 - confidence) * sorted.Count);
            index = Math.Clamp(index, 0, sorted.Count - 1);

            return sorted[index];
        }

        #region 纯数学指标计算

        private decimal CalculateSimpleMovingAverage(List<decimal> prices, MovingAverageType movingAverageType)
        {
            int periodValue = (int)movingAverageType;

            if (prices.Count < periodValue)
                return prices.Count == 0 ? 0 : prices.Average();

            return prices.TakeLast(periodValue).Average();
        }

        private decimal CalculateRSI(List<decimal> prices, int period)
        {
            if (prices.Count < period + 1)
                return 0m;

            decimal avgGain = 0m;
            decimal avgLoss = 0m;

            for (int i = 1; i <= period; i++)
            {
                decimal change = prices[i] - prices[i - 1];
                if (change >= 0)
                    avgGain += change;
                else
                    avgLoss -= change;
            }
            avgGain /= period;
            avgLoss /= period;

            for (int i = period + 1; i < prices.Count; i++)
            {
                decimal change = prices[i] - prices[i - 1];
                if (change >= 0)
                {
                    avgGain = (avgGain * (period - 1) + change) / period;
                    avgLoss = (avgLoss * (period - 1)) / period;
                }
                else
                {
                    avgGain = (avgGain * (period - 1)) / period;
                    avgLoss = (avgLoss * (period - 1) - change) / period;
                }
            }

            if (avgLoss == 0)
                return 100m;

            decimal rs = avgGain / avgLoss;
            decimal rsi = 100 - (100 / (1 + rs));
            return Math.Round(rsi, 2);
        }

        private decimal CalculateVolatility(List<decimal> prices)
        {
            if (prices.Count < 2)
                return 0m;

            var returns = new List<decimal>();
            for (int i = 1; i < prices.Count; i++)
            {
                if (prices[i - 1] != 0)
                    returns.Add((prices[i] - prices[i - 1]) / prices[i - 1]);
            }

            if (returns.Count == 0)
                return 0m;

            decimal avg = returns.Average();
            decimal variance = returns.Select(r => (r - avg) * (r - avg)).Average();
            decimal dailyStd = (decimal)Math.Sqrt((double)variance);
            decimal annualizedVol = dailyStd * (decimal)Math.Sqrt(252); // Assume there are 252 trading days in a year

            return Math.Round(annualizedVol, 4);
        }

        #endregion

        #region ML.NET 回归预测（次日收盘价）

        private decimal PredictNextPrice(List<StockData> historicalData)
        {
            try
            {
                var trainData = new List<PricePredictionInput>();
                for (int i = 5; i < historicalData.Count - 1; i++)
                {
                    trainData.Add(new PricePredictionInput
                    {
                        PriceMinus5 = (float)historicalData[i - 5].ExecutionPrice,
                        PriceMinus4 = (float)historicalData[i - 4].ExecutionPrice,
                        PriceMinus3 = (float)historicalData[i - 3].ExecutionPrice,
                        PriceMinus2 = (float)historicalData[i - 2].ExecutionPrice,
                        PriceMinus1 = (float)historicalData[i - 1].ExecutionPrice,
                        Volume = (float)historicalData[i].Quantity,
                        NextPrice = (float)historicalData[i].ExecutionPrice
                    });
                }

                if (trainData.Count < 10)
                    return 0m;

                IDataView dataView = _mlContext.Data.LoadFromEnumerable(trainData);
                var pipeline = _mlContext.Transforms
                    .Concatenate("Features",
                        nameof(PricePredictionInput.PriceMinus5),
                        nameof(PricePredictionInput.PriceMinus4),
                        nameof(PricePredictionInput.PriceMinus3),
                        nameof(PricePredictionInput.PriceMinus2),
                        nameof(PricePredictionInput.PriceMinus1),
                        nameof(PricePredictionInput.Volume))
                    .Append(_mlContext.Regression.Trainers.Sdca(
                        labelColumnName: nameof(PricePredictionInput.NextPrice),
                        featureColumnName: "Features"));

                var model = pipeline.Fit(dataView);
                var last = historicalData.Last();
                var input = new PricePredictionInput
                {
                    PriceMinus5 = (float)historicalData[^5].ExecutionPrice,
                    PriceMinus4 = (float)historicalData[^4].ExecutionPrice,
                    PriceMinus3 = (float)historicalData[^3].ExecutionPrice,
                    PriceMinus2 = (float)historicalData[^2].ExecutionPrice,
                    PriceMinus1 = (float)historicalData[^1].ExecutionPrice,
                    Volume = (float)last.Quantity
                };
                var predictionEngine = _mlContext.Model.CreatePredictionEngine<PricePredictionInput, PricePredictionOutput>(model);
                var prediction = predictionEngine.Predict(input); 
                decimal rawPrice = (decimal)prediction.PredictedPrice;

                // 确保价格不为负数，且不低于最小合理价格（例如 0.01）
                decimal clampedPrice = Math.Max(0.01m, rawPrice);

                if (rawPrice < 0)
                {
                    _logger?.LogWarning("ML.NET 预测出负价格 {RawPrice}，已裁剪为 {ClampedPrice}", rawPrice, clampedPrice);
                }

                return clampedPrice;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "ML.NET 价格预测失败");
                return 0m;
            }
        }

        #endregion

        #region ML.NET 异常检测（IID 更改点）

        private (bool IsAnomaly, string Alert) DetectPriceChangePoint(List<StockData> historicalData)
        {
            if (historicalData.Count < 10)
                return (false, "数据不足，无法检测异常");

            try
            {
                var prices = historicalData.Select(d => (float)d.ExecutionPrice).ToList();

                // 创建数据视图
                var inputData = prices.Select(p => new PriceData { Price = p });
                var dataView = _mlContext.Data.LoadFromEnumerable(inputData);

                // 更改点检测估算器
                var estimator = _mlContext.Transforms.DetectIidChangePoint(
                    outputColumnName: nameof(ChangePointOutput.Prediction),
                    inputColumnName: nameof(PriceData.Price),
                    confidence: 95.0,
                    changeHistoryLength: Math.Max(7, prices.Count / 10));  // 窗口长度

                // 训练转换器（时序检测器需要 Fit 传入空数据或原始数据，这里 Fit 原始数据即可）
                var transformer = estimator.Fit(dataView);
                var transformedData = transformer.Transform(dataView);
                var predictions = _mlContext.Data.CreateEnumerable<ChangePointOutput>(transformedData, reuseRowObject: false).ToList();

                var last = predictions.LastOrDefault();
                if (last != null && last.Prediction != null && last.Prediction.Length >= 1 && last.Prediction[0] > 0)
                {
                    return (true, $"检测到价格模式发生持久性改变 (置信度95%)");
                }

                return (false, "价格模式稳定");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "异常检测失败");
                return (false, "检测失败");
            }
        }

        #endregion

        #region ML.NET 聚类（风险等级分类）

        // 辅助风险指标方法
        public decimal CalculateSharpeRatio(List<decimal> returns, decimal riskFreeRate = 0.02m)
        {
            if (returns == null || returns.Count < 2) 
                return 0m;

            var avgReturn = returns.Average();
            var std = Math.Sqrt(returns.Select(r => Math.Pow((double)(r - avgReturn), 2)).Average());

            if (std == 0)
                return 0m;

            return (avgReturn - riskFreeRate) / (decimal)std * (decimal)Math.Sqrt(252);
        }

        public decimal CalculateMaxDrawdown(List<decimal> navSeries)
        {
            if (navSeries == null || navSeries.Count < 2) 
                return 0m;

            decimal peak = navSeries[0];
            decimal maxDrawdown = 0m;
            foreach (var val in navSeries)
            {
                if (val > peak) 
                    peak = val;

                var drawdown = (peak - val) / peak;

                if (drawdown > maxDrawdown)
                    maxDrawdown = drawdown;
            }

            return maxDrawdown;
        }

        private (RiskCluster RiskCluster, string Description) ClassifyRiskCluster(List<StockData> historicalData)
        {
            try
            {
                // 计算特征向量：[波动率, RSI, 夏普比率, 最大回撤]
                var prices = historicalData.Select(d => d.ExecutionPrice).ToList();
                var returns = new List<decimal>();
                for (int i = 1; i < prices.Count; i++)
                {
                    if (prices[i - 1] != 0)
                        returns.Add((prices[i] - prices[i - 1]) / prices[i - 1]);
                }

                float volatility = (float)CalculateVolatility(prices);
                float rsi = (float)CalculateRSI(prices, 14);
                float sharpe = (float)CalculateSharpeRatio(returns);
                float maxDrawdown = (float)CalculateMaxDrawdown(prices);

                float[] features = [ volatility, rsi, sharpe, maxDrawdown ];
                var data = _mlContext.Data.LoadFromEnumerable([ new RiskFeatures { Features = features } ]);

                // K-Means 训练管道
                var pipeline = _mlContext.Transforms
                    .Concatenate("Features", nameof(RiskFeatures.Features))
                    .Append(_mlContext.Clustering.Trainers.KMeans(
                        featureColumnName: "Features",
                        numberOfClusters: 3));

                var model = pipeline.Fit(data);
                var predictionEngine = _mlContext.Model.CreatePredictionEngine<RiskFeatures, ClusterPrediction>(model);
                var prediction = predictionEngine.Predict(new RiskFeatures { Features = features });

                RiskCluster RiskCluster = (RiskCluster)(prediction.PredictedLabel + 1); // 转为 1~3
                string desc = RiskCluster switch
                {
                    RiskCluster.Low => "低风险（波动小、回撤低）",
                    RiskCluster.Medium => "中风险（均衡型）",
                    RiskCluster.High => "高风险（高波动、大回撤）",
                    _ => "未知"
                };

                return (RiskCluster, desc);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "风险聚类失败");
                return (0, "分类失败");
            }
        }

        #endregion
    }
}
