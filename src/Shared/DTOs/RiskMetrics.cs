using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.DTOs
{
    public class RiskMetrics
    {
        /// <summary>最大回撤 (百分比，如0.15表示15%)</summary>
        public decimal MaxDrawdown { get; set; }

        /// <summary>夏普比率 (年化)</summary>
        public decimal SharpeRatio { get; set; }

        /// <summary>95% 置信度下的在险价值 (VaR)，绝对值或百分比</summary>
        public decimal VaR95 { get; set; }
    }
}
