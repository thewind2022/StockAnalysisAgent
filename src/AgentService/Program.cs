using AgentService.Agent;
using AgentService.Services;
using Infrastructure.Resilience;
using Infrastructure.StockRepository;
using Microsoft.OpenApi;
using Microsoft.SemanticKernel;
using ML.Service.Extensions;
using Polly;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog 配置
builder.Host.UseSerilog((context, services, config) =>
{
    //config.ReadFrom.Configuration(context.Configuration)
    //      .Enrich.FromLogContext()
    //      .WriteTo.Console()
    //      .WriteTo.File("logs/agent-.log", rollingInterval: RollingInterval.Day);

    config.ReadFrom.Configuration(context.Configuration)
         .ReadFrom.Services(services)
         .Enrich.FromLogContext();
});

// 2. 注册 MVC 控制器
builder.Services.AddControllers();

// 3. 注册 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Stock Agent API",
        Version = "v1",
        Description = "AI 驱动的投资分析 Agent API - 支持股票查询、技术指标、备忘录生成"
    });
    // 可选：添加 XML 注释文件路径（如果生成）
    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // c.IncludeXmlComments(xmlPath);
});

// 4. 注册其他服务（Repository, HttpClient, ML, Semantic Kernel）
//builder.Services.AddTransient<IStockRepository, StockRepository>();
builder.Services.AddTransient<IStockRepository, MockRepository>();
builder.Services.AddHttpClient("StockApiClient")
    .AddPolicyHandler((sp, _) =>
    {
        var logger = sp.GetRequiredService<ILogger<Program>>();
        var retry = ResiliencePolicies.GetRetryPolicy(logger);
        var breaker = ResiliencePolicies.GetCircuitBreakerPolicy(logger);
        var bulkhead = ResiliencePolicies.GetBulkheadPolicy(10, 5, logger);
        return Policy.WrapAsync(bulkhead, retry, breaker);
    });

builder.Services.AddMLServices();
builder.Services.AddSingleton<StockAgentBuilder>();
builder.Services.AddSingleton<Kernel>(sp => sp.GetRequiredService<StockAgentBuilder>().BuildKernelAsync().Result);
builder.Services.AddTransient<StockService>();
builder.Services.AddTransient<MemoService>();

var app = builder.Build();

// 5. Swagger 中间件（放在 UseRouting 之前或之后通常都可以，但建议在异常处理和 HTTPS 之后）
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Stock Agent API v1"));
}

app.UseSerilogRequestLogging();

// 可选：HTTPS 重定向（如需要）
// app.UseHttpsRedirection();

app.MapControllers();
app.Run();