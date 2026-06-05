
using Shared.Enums;

namespace Shared.DTOs;
public class TechnicalIndicator
{
    public decimal MovingAverage5 { get; set; }
    public decimal MovingAverage10 { get; set; }
    public decimal MovingAverage20 { get; set; }
    public decimal MovingAverage30 { get; set; }
    public decimal MovingAverage60 { get; set; }
    public decimal MovingAverage120 { get; set; }

    public decimal RSI { get; set; }
    public decimal Volatility { get; set; }

    public decimal? PredictedNextPrice { get; set; }


    // 新增：异常检测结果
    public bool IsPriceAnomaly { get; set; }
    public string? AnomalyAlert { get; set; }


    // 新增：聚类风险等级
    public RiskCluster RiskCluster { get; set; }
    public string? RiskClusterDescription { get; set; }
}