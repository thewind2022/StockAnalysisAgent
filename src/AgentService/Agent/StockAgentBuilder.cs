using Microsoft.SemanticKernel;

namespace AgentService.Agent
{
    public class StockAgentBuilder
    {
        private readonly IConfiguration _config;

        public StockAgentBuilder(IConfiguration config)
        {
            _config = config;
        }

        public async Task<Kernel> BuildKernelAsync()
        {
            var builder = Kernel.CreateBuilder();
            var endpoint = _config["OpenAI:Endpoint"];
            var apiKey = _config["OpenAI:ApiKey"];
            var modelId = _config["OpenAI:ModelId"];

            builder.AddOpenAIChatCompletion(modelId, new Uri(endpoint), apiKey);

            builder.Services.AddLogging(l => l.AddConsole());

            var kernel = builder.Build();

            return await Task.FromResult(kernel);
        }
    }
}
