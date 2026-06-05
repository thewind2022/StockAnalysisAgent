namespace Shared.Services
{
    public interface IOnnxPredictionService
    {
        Task<float[]> PredictAsync(long[] inputTokenIds);
        bool IsAvailable { get; }
        void Dispose();
    }
}
