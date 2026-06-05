
namespace Shared.DTOs;
public class InvestmentMemo
{
    public string SecurityCode { get; set; }
    public string SecurityDesc { get; set; }
    public string Recommendation { get; set; }
    public decimal DtdPnlTotal { get; set; }
    public decimal DtdRealizedPnl { get; set; }
    public decimal DtdUnrealizedPnl { get; set; }
    public decimal MarketValue { get; set; }
    public decimal RiskScore { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}