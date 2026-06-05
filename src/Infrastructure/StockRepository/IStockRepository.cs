using Shared.DTOs;
namespace Infrastructure.StockRepository
{
    public interface IStockRepository
    {
        Task<StockData?> GetStockByCodeAsync(string code, DateTime date);
        Task<IEnumerable<StockData>> GetAllStocksAsync();
    }

}
