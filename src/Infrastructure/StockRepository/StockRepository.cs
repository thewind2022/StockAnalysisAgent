using Polly;
using Polly.Retry;
using Shared.Constants;
using Shared.DTOs;
using System.Text.Json;

namespace Infrastructure.StockRepository
{
    public class StockRepository : IStockRepository
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        public StockRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // 结合Polly重试策略
            _retryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(msg => !msg.IsSuccessStatusCode)
                .WaitAndRetryAsync(Constants.MAX_RETRY_ATTEMPTS,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        public async Task<IEnumerable<StockData>> GetAllStocksAsync()
        {
            var response = await _retryPolicy.ExecuteAsync(() =>
                _httpClient.GetAsync($"https://api.example.com/stocks/"));

            if (!response.IsSuccessStatusCode) 
                return null;

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<StockData>>(json);
        }

        public async Task<StockData?> GetStockByCodeAsync(string code, DateTime date)
        {
            var response = await _retryPolicy.ExecuteAsync(() =>
                _httpClient.GetAsync($"https://api.example.com/stocks/{code}?date={date:yyyy-MM-dd}"));

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<StockData>(json);
        }
    }
}
