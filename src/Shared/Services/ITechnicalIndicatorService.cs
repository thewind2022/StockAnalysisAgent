using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Services
{
    public interface ITechnicalIndicatorService
    {
        TechnicalIndicator CalculateIndicators(List<StockData> historicalData);
        decimal CalculateMaxDrawdown(List<decimal> navSeries);
        decimal CalculateSharpeRatio(List<decimal> returns, decimal riskFreeRate = 0.02m);
        decimal CalculateVaR(List<decimal> returns, double confidence = 0.95);
    }
}
