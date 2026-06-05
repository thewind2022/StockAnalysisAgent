using Shared.DTOs;
namespace Infrastructure.Mock
{
    public static class MockDataSeeder
    {
        private static readonly List<StockData> _mockStocks;

        static MockDataSeeder()
        {
            _mockStocks = new List<StockData>
            {
                new()
                {
                    SecurityCode = "000001",
                    SecurityDesc = "平安银行",
                    Date = DateTime.Today.Date,
                    NetMoney = 1000000,
                    Quantity = 50000,
                    ExecutionPrice = 11.50m,
                    Commissions = 100m,
                    FeeTax = 0.03m,
                    SettlementMoney = 999000m,
                    SubgroupCode = "GROUP_A",
                    PortfolioCode = "PORT_A"
                },
                new() { SecurityCode = "600036", SecurityDesc = "招商银行", Date = DateTime.Today,
                        NetMoney = 2000000, Quantity = 100000, ExecutionPrice = 33.20m, Commissions = 200m,
                        FeeTax = 0.03m, SettlementMoney = 1998000m, SubgroupCode = "GROUP_B",
                        PortfolioCode = "PORT_B" },
                new() { SecurityCode = "000858", SecurityDesc = "五粮液", Date = DateTime.Today,
                        NetMoney = 1500000, Quantity = 20000, ExecutionPrice = 168.50m, Commissions = 150m,
                        FeeTax = 0.03m, SettlementMoney = 1498500m, SubgroupCode = "GROUP_C",
                        PortfolioCode = "PORT_C" }
            };
        }
        public static List<StockData> GetMockStocks()
        {
            return _mockStocks;
        }
    }
}
