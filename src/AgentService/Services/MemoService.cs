
using Microsoft.SemanticKernel;
using Shared.DTOs;

namespace AgentService.Services
{

    /// <summary>
    /// 投资备忘录插件（供 Controller 直接调用，内部使用 Kernel 生成 AI 建议）
    /// </summary>
    public class MemoService
    {
        private readonly Kernel _kernel;
        private readonly ILogger<MemoService> _logger;
        private readonly List<InvestmentMemo> _memos = new();

        public MemoService(Kernel kernel, ILogger<MemoService> logger)
        {
            _kernel = kernel;
            _logger = logger;
        }

        /// <summary>
        /// 生成并保存投资备忘录
        /// </summary>
        public async Task<InvestmentMemo> GenerateMemoAsync(
            string code,
            string name,
            decimal dtdPnl,
            decimal realizedPnl,
            decimal unrealizedPnl,
            decimal marketValue,
            TechnicalIndicator indicators)
        {
            _logger.LogInformation("开始生成备忘录: {Code} {Name}", code, name);

            var riskScore = CalculateRiskScore(indicators, unrealizedPnl, marketValue);
            var recommendation = await GenerateAIRecommendationAsync(code, name, dtdPnl, indicators, riskScore);

            var memo = new InvestmentMemo
            {
                SecurityCode = code,
                SecurityDesc = name,
                DtdPnlTotal = dtdPnl,
                DtdRealizedPnl = realizedPnl,
                DtdUnrealizedPnl = unrealizedPnl,
                MarketValue = marketValue,
                RiskScore = riskScore,
                Recommendation = recommendation,
                GeneratedAt = DateTime.UtcNow
            };

            _memos.Add(memo);
            _logger.LogInformation("备忘录生成完成: {Code}, 风险分: {RiskScore}, 建议: {Recommendation}",
                code, riskScore, recommendation);
            return memo;
        }

        private decimal CalculateRiskScore(TechnicalIndicator indicators, decimal unrealizedPnl, decimal marketValue)
        {
            decimal score = 0m;

            if (indicators.RSI > 70)
                score += (indicators.RSI - 70) / 30 * 30;
            else if (indicators.RSI < 30)
                score -= 10;

            if (indicators.Volatility > 0.3m)
                score += Math.Min(30, (indicators.Volatility - 0.3m) / 0.2m * 30);

            if (marketValue != 0)
            {
                var unrealizedRatio = Math.Abs(unrealizedPnl / marketValue);
                if (unrealizedPnl < 0 && unrealizedRatio > 0.2m)
                    score += Math.Min(20, unrealizedRatio * 100);
            }

            return Math.Clamp(score, 0, 100);
        }

        private async Task<string> GenerateAIRecommendationAsync(
            string code,
            string name,
            decimal dtdPnl,
            TechnicalIndicator indicators,
            decimal riskScore)
        {
            var prompt = $@"
你是一位经验丰富的投资顾问。根据以下股票数据，生成一段简洁、专业的投资建议（50字以内）：
- 股票：{code} {name}
- 今日盈亏：{(dtdPnl >= 0 ? "盈利" : "亏损")} {Math.Abs(dtdPnl):F2} 元
- 当前RSI：{indicators.RSI:F1}（{(indicators.RSI > 70 ? "超买区" : indicators.RSI < 30 ? "超卖区" : "中性区")}）
- 年化波动率：{indicators.Volatility:P1}
- 综合风险评分：{riskScore:F0}/100

要求：
1. 直接输出建议，不要有任何额外解释。
2. 建议应包含操作方向（买入/卖出/持有）、核心理由、风险提示（如有）。
3. 语气专业，避免主观评价。
4. 如果风险分>70，建议以“减仓”或“止损”为主；如果风险分<30，可考虑“持有”或“逢低布局”。
";

            try
            {
                var result = await _kernel.InvokePromptAsync(prompt);
                var recommendation = result.GetValue<string>()?.Trim();
                if (string.IsNullOrWhiteSpace(recommendation))
                {
                    _logger.LogWarning("LLM 返回空建议，使用默认文案");
                    return "数据不足，建议关注市场动态，谨慎操作。";
                }
                return recommendation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用 LLM 生成建议失败");
                return "AI 服务暂时不可用，请稍后重试或参考技术指标自行判断。";
            }
        }
    }
}
