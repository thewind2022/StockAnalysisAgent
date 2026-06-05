using Microsoft.ML;
using Shared.DTOs;
using Shared.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ML.Service.Services
{
    public class TechnicalIndicatorService_Ori //: ITechnicalIndicatorService
    {
        //private readonly MLContext _mlContext = new();

        #region 技术指标，风险指标

        public TechnicalIndicator CalculateIndicators(List<StockData> historicalData)
        {
            if (historicalData == null || historicalData.Count == 0)
                return new TechnicalIndicator();

            var prices = historicalData.Select(d => d.ExecutionPrice).ToList();
            return new TechnicalIndicator
            {
                MovingAverage20 = CalculateSimpleMovingAverage(prices, 20),
                MovingAverage60 = CalculateSimpleMovingAverage(prices, 60),
                RSI = CalculateRSI(prices, 14),
                Volatility = CalculateVolatility(prices)
            };
        }


        public decimal CalculateMaxDrawdown(List<decimal> navSeries)
        {
            if (navSeries == null || navSeries.Count < 2) return 0m;
            decimal peak = navSeries[0];
            decimal maxDrawdown = 0m;
            foreach (var val in navSeries)
            {
                if (val > peak) peak = val;
                var drawdown = (peak - val) / peak;
                if (drawdown > maxDrawdown) maxDrawdown = drawdown;
            }
            return maxDrawdown;
        }

        public decimal CalculateSharpeRatio(List<decimal> returns, decimal riskFreeRate = 0.02m)
        {
            if (returns == null || returns.Count < 2) return 0m;
            var avgReturn = returns.Average();
            var std = Math.Sqrt(returns.Select(r => Math.Pow((double)(r - avgReturn), 2)).Average());
            if (std == 0) return 0m;
            return (avgReturn - riskFreeRate) / (decimal)std * (decimal)Math.Sqrt(252); // 年化
        }

        #endregion

        private decimal CalculateSimpleMovingAverage(List<decimal> prices, int period)
        {
            if (prices.Count < period)
                return prices.Count == 0 ? 0 : prices.Average();
            return prices.TakeLast(period).Average();
        }

        /// <summary>
        /// 计算相对强弱指数 (RSI)
        /// </summary>
        /// <param name="prices">价格序列（按时间顺序，最新的在最后）</param>
        /// <param name="period">计算周期，通常为14</param>
        /// <returns>RSI值（0-100）</returns>
        private decimal CalculateRSI(List<decimal> prices, int period)
        {
            if (prices.Count < period + 1)
                return 0m; // 数据不足，返回中性值

            decimal avgGain = 0m;
            decimal avgLoss = 0m;

            // 计算初始平均涨跌幅
            for (int i = 1; i <= period; i++)
            {
                decimal change = prices[i] - prices[i - 1];
                if (change >= 0)
                    avgGain += change;
                else
                    avgLoss -= change; // 转为正数
            }
            avgGain /= period;
            avgLoss /= period;

            // 平滑计算后续值
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
                    avgLoss = (avgLoss * (period - 1) - change) / period; // change为负，-change为正
                }
            }

            if (avgLoss == 0)
                return 100m; // 一直上涨，RSI=100

            decimal rs = avgGain / avgLoss;
            decimal rsi = 100 - (100 / (1 + rs));
            return Math.Round(rsi, 2);
        }

        /// <summary>
        /// 计算价格波动率（标准差）
        /// </summary>
        /// <param name="prices">价格序列</param>
        /// <returns>年化波动率（默认按日度数据，年化因子sqrt(252)）</returns>
        private decimal CalculateVolatility(List<decimal> prices)
        {
            if (prices.Count < 2)
                return 0m;

            // 计算日收益率
            var returns = new List<decimal>();
            for (int i = 1; i < prices.Count; i++)
            {
                if (prices[i - 1] != 0)
                    returns.Add((prices[i] - prices[i - 1]) / prices[i - 1]);
            }

            if (returns.Count == 0)
                return 0m;

            // 计算平均收益率
            decimal mean = returns.Average();
            // 计算方差
            decimal variance = returns.Select(r => (r - mean) * (r - mean)).Average();
            // 标准差（日度）
            decimal dailyStd = (decimal)Math.Sqrt((double)variance);
            // 年化（假设交易日252天）
            decimal annualizedVol = dailyStd * (decimal)Math.Sqrt(252);
            return Math.Round(annualizedVol, 4);
        }
    }
}
