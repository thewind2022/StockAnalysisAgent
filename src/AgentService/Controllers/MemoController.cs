using AgentService.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared.Services;

namespace AgentService.Controllers
{

    [ApiController]
    [Route("api/agent/[action]")]
    public class MemoController : ControllerBase
    {
        private readonly StockService _StockService;
        private readonly MemoService _MemoService;
        private readonly ITechnicalIndicatorService _indicatorService;
        private readonly ILogger<MemoController> _logger;

        public MemoController(
            StockService StockService,
            MemoService MemoService,
            ITechnicalIndicatorService indicatorService,
            ILogger<MemoController> logger)
        {
            _StockService = StockService;
            _MemoService = MemoService;
            _indicatorService = indicatorService;
            _logger = logger;
        }

        /// <summary>
        /// 生成投资备忘录
        /// </summary>
        /// <param name="request">股票代码和日期</param>
        /// <returns>投资备忘录对象</returns>
        [HttpPost]
        [ProducesResponseType(typeof(InvestmentMemo), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<InvestmentMemo>> GenerateMemo([FromBody] MemoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.StockCode))
            {
                return BadRequest("股票代码不能为空");
            }

            _logger.LogInformation("收到生成备忘录请求: 股票={StockCode}, 日期={Date}", request.StockCode, request.Date);

            // 1. 获取股票数据（当前或指定日期的快照）
            var stockData = await _StockService.GetStockAsync(request.StockCode, request.Date);
            if (stockData == null)
            {
                _logger.LogWarning("未找到股票数据: {StockCode}", request.StockCode);
                return NotFound($"未找到股票代码 {request.StockCode} 在 {request.Date:yyyy-MM-dd} 的数据");
            }

            // 2. 获取历史数据用于技术指标计算
            // 实际场景中应从仓储获取多日历史价格，此处简化使用当前单条数据（指标计算会退化为默认值）
            var historicalData = new List<StockData> { stockData };
            var indicators = _indicatorService.CalculateIndicators(historicalData);

            // 3. 生成备忘录（盈亏等字段需要根据真实持仓计算，这里仅做示例，实际应从持仓服务获取）
            // 注意：dtdPnl, realizedPnl, unrealizedPnl, marketValue 应根据实际业务逻辑计算
            // 以下示例使用占位值
            var memo = await _MemoService.GenerateMemoAsync(
                code: stockData.SecurityCode,
                name: stockData.SecurityDesc,
                dtdPnl: 0m,                    // 示例：今日总盈亏
                realizedPnl: 0m,              // 示例：已实现盈亏
                unrealizedPnl: 0m,            // 示例：未实现盈亏
                marketValue: stockData.NetMoney,
                indicators: indicators
            );

            return Ok(memo);
        }
    }

    /// <summary>
    /// 生成备忘录请求参数
    /// </summary>
    public class MemoRequest
    {
        /// <summary>
        /// 股票代码，如 600036
        /// </summary>
        public string StockCode { get; set; } = string.Empty;

        /// <summary>
        /// 查询日期，格式 yyyy-MM-dd
        /// </summary>
        public DateTime Date { get; set; } = DateTime.Today;
    }

}
