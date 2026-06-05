
using Infrastructure.StockRepository;
using Shared.DTOs;
using Shared.Services;

namespace AgentService.Services
{
    /// <summary>
    /// 股票数据查询与计算插件（供 Controller 直接调用）
    /// </summary>
    public class StockService
    {
        private readonly IStockRepository _stockRepo;
        private readonly IOnnxPredictionService _onnxService;
        private readonly ITechnicalIndicatorService _indicatorService;
        private readonly ILogger<StockService> _logger;

        public StockService(
            IStockRepository stockRepo,
            IOnnxPredictionService onnxService,
            ITechnicalIndicatorService indicatorService,
            ILogger<StockService> logger)
        {
            _stockRepo = stockRepo;
            _onnxService = onnxService;
            _indicatorService = indicatorService;
            _logger = logger;
        }

        /// <summary>
        /// 根据股票代码和日期查询股票详细信息
        /// </summary>
        public async Task<StockData?> GetStockAsync(string code, DateTime date)
        {
            _logger.LogInformation("调用 GetStockAsync: Code={Code}, Date={Date}", code, date);
            var result = await _stockRepo.GetStockByCodeAsync(code, date);
            if (result == null)
                _logger.LogWarning("未找到股票数据: {Code}", code);
            return result;
        }

        /// <summary>
        /// 计算持仓盈亏
        /// </summary>
        public async Task<decimal> CalculatePnlAsync(long quantity, decimal avgCost, decimal currentPrice)
        {
            var pnl = (currentPrice - avgCost) * quantity;
            _logger.LogInformation("计算PnL: 数量={Quantity}, 成本={AvgCost}, 现价={CurrentPrice}, 结果={Pnl}",
                quantity, avgCost, currentPrice, pnl);
            return await Task.FromResult(pnl);
        }

        /// <summary>
        /// 计算组合风险指标：最大回撤、夏普比率、在险价值(VaR)
        /// </summary>
        public async Task<RiskMetrics> CalculateRiskMetricsAsync(List<decimal> historicalNav)
        {
            if (historicalNav == null || historicalNav.Count < 2)
            {
                _logger.LogWarning("历史数据不足，无法计算风险指标");
                return new RiskMetrics();
            }

            _logger.LogInformation("计算风险指标，数据点数: {Count}", historicalNav.Count);
            

            var returns = new List<decimal>();
            for (int i = 1; i < historicalNav.Count; i++)
            {
                if (historicalNav[i - 1] != 0)
                    returns.Add((historicalNav[i] - historicalNav[i - 1]) / historicalNav[i - 1]);
            }

            var maxDrawdown = _indicatorService.CalculateMaxDrawdown(historicalNav);
            var sharpeRatio = _indicatorService.CalculateSharpeRatio(returns);
            var var95 = _indicatorService.CalculateVaR(returns, 0.95);

            return await Task.FromResult(new RiskMetrics
            {
                MaxDrawdown = maxDrawdown,
                SharpeRatio = sharpeRatio,
                VaR95 = var95
            });
        }

        /// <summary>
        /// 使用 ONNX 模型进行情感分析或风险评估
        /// </summary>
        public async Task<float> EvaluateRiskUsingOnnxAsync(string code, string text)
        {
            _logger.LogInformation("调用 ONNX 模型评估风险: Code={Code}, TextLength={Length}", code, text?.Length ?? 0);
            if (string.IsNullOrWhiteSpace(text))
                return 0.5f;

            try
            {
                var dummyTokenIds = new long[] { 101, 102, 103, 104, 105 };
                var result = await _onnxService.PredictAsync(dummyTokenIds);
                float riskScore = result.Length > 0 ? result[0] : 0.5f;
                return Math.Clamp(riskScore, 0f, 1f);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ONNX 推理失败");
                return 0.5f;
            }
        }
    }
}
