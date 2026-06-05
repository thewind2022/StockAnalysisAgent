
namespace Shared.DTOs;
public class StockData
{
    public string SubgroupCode { get; set; }
    public string PortfolioCode { get; set; }
    public string SecurityCode { get; set; }
    public string SecurityDesc { get; set; }
    public DateTime Date { get; set; }
    public long Quantity { get; set; }
    public decimal NetMoney { get; set; }
    public decimal ExecutionPrice { get; set; }
    public decimal Commissions { get; set; }
    public decimal FeeTax { get; set; }
    public decimal SettlementMoney { get; set; }
}