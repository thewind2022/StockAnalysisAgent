using Infrastructure.Mock;
using Shared.DTOs;

namespace Infrastructure.StockRepository
{
    public class MockRepository : IStockRepository
    {
        public async Task<IEnumerable<StockData>> GetAllStocksAsync()
        {
            return MockDataSeeder.GetMockStocks();
        }

        public async Task<StockData?> GetStockByCodeAsync(string code, DateTime date)
        {
            return MockDataSeeder.GetMockStocks()
                .FirstOrDefault(s => s.SecurityCode == code && s.Date == date.Date);
        }
    }
}
