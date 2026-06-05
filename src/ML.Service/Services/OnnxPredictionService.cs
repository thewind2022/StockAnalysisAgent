using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Shared.Services;

namespace ML.Service.Services
{
    public class OnnxPredictionService : IOnnxPredictionService, IDisposable
    {
        private readonly InferenceSession? _session;
        private readonly ILogger<OnnxPredictionService> _logger;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly bool _isAvailable;

        public OnnxPredictionService(IConfiguration config, ILogger<OnnxPredictionService> logger)
        {
            _logger = logger;
            var modelPath = config["Onnx:ModelPath"] ?? "Models/sentiment_model.onnx";
            modelPath = Path.Combine(AppContext.BaseDirectory, modelPath);

            if (!File.Exists(modelPath))
            {
                _logger.LogWarning("ONNX 模型文件不存在: {ModelPath}，ONNX 推理功能将不可用。", modelPath);
                _isAvailable = false;
                return;
            }

            try
            {
                _session = new InferenceSession(modelPath);
                _isAvailable = true;
                _logger.LogInformation("ONNX 模型加载成功: {ModelPath}", modelPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ONNX 模型加载失败: {ModelPath}", modelPath);
                _isAvailable = false;
            }
        }

        public bool IsAvailable => _isAvailable;

        public async Task<float[]> PredictAsync(long[] inputTokenIds)
        {
            if (!_isAvailable || _session == null)
            {
                _logger.LogWarning("ONNX 服务不可用，返回默认预测值。");
                return await Task.FromResult(new float[] { 0.5f });
            }

            await _semaphore.WaitAsync();
            try
            {
                var inputTensor = new DenseTensor<long>(inputTokenIds, new[] { 1, inputTokenIds.Length });
                var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", inputTensor)
            };
                using var results = _session.Run(inputs);
                var outputTensor = results.First().AsTensor<float>();
                return outputTensor.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ONNX 推理失败");
                return new float[] { 0.5f };
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            _session?.Dispose();
            _semaphore?.Dispose();
        }
    }
}
