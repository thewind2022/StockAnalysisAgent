<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Stock Analysis Agent – AI-Powered Investment Assistant | .NET Core 10</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', 'Roboto', 'Helvetica Neue', sans-serif;
            background: #f5f7fb;
            color: #1e293b;
            line-height: 1.5;
            padding: 2rem 1rem;
        }

        .container {
            max-width: 1280px;
            margin: 0 auto;
            background: white;
            border-radius: 28px;
            box-shadow: 0 20px 35px -12px rgba(0,0,0,0.1);
            overflow: hidden;
        }

        .hero {
            background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
            color: white;
            padding: 3rem 2rem;
        }

            .hero h1 {
                font-size: 2.5rem;
                font-weight: 700;
                margin-bottom: 0.75rem;
                letter-spacing: -0.02em;
            }

            .hero p {
                font-size: 1.2rem;
                opacity: 0.85;
                max-width: 800px;
            }

        .badge {
            display: inline-block;
            background: rgba(255,255,255,0.15);
            backdrop-filter: blur(4px);
            padding: 0.25rem 0.75rem;
            border-radius: 40px;
            font-size: 0.8rem;
            font-weight: 500;
            margin-top: 1rem;
        }

        .content {
            padding: 2rem 2rem 3rem;
        }

        h2 {
            font-size: 1.8rem;
            font-weight: 600;
            margin: 1.8rem 0 1rem;
            border-left: 5px solid #3b82f6;
            padding-left: 1rem;
        }

            h2:first-of-type {
                margin-top: 0;
            }

        h3 {
            font-size: 1.3rem;
            font-weight: 600;
            margin: 1.5rem 0 0.75rem;
            color: #0f172a;
        }

        .grid-2, .grid-3 {
            display: grid;
            gap: 1.5rem;
            margin: 1.5rem 0;
        }

        .grid-2 {
            grid-template-columns: repeat(auto-fit, minmax(280px,1fr));
        }

        .grid-3 {
            grid-template-columns: repeat(auto-fit, minmax(260px,1fr));
        }

        .card {
            background: #f8fafc;
            border-radius: 20px;
            padding: 1.5rem;
            border: 1px solid #e2e8f0;
            transition: all 0.2s;
        }

            .card:hover {
                transform: translateY(-3px);
                box-shadow: 0 8px 20px rgba(0,0,0,0.05);
                border-color: #cbd5e1;
            }

            .card h4 {
                font-size: 1.2rem;
                font-weight: 600;
                margin-bottom: 0.75rem;
                display: flex;
                align-items: center;
                gap: 0.5rem;
            }

            .card p {
                color: #334155;
                font-size: 0.95rem;
            }

        .tech-badge {
            background: #e2e8f0;
            color: #1e293b;
            border-radius: 30px;
            padding: 0.2rem 0.7rem;
            font-size: 0.75rem;
            font-weight: 500;
            display: inline-block;
            margin: 0.25rem 0.25rem 0 0;
        }

        .architecture {
            background: #f1f5f9;
            border-radius: 20px;
            padding: 1.5rem;
            text-align: center;
            font-family: monospace;
            font-size: 0.85rem;
            line-height: 1.8;
            overflow-x: auto;
            white-space: pre;
            margin: 1.5rem 0;
        }

        table {
            width: 100%;
            border-collapse: collapse;
            margin: 1.2rem 0;
        }

        th, td {
            border: 1px solid #e2e8f0;
            padding: 0.75rem;
            text-align: left;
            vertical-align: top;
        }

        th {
            background: #f1f5f9;
            font-weight: 600;
        }

        .highlight {
            background: #eff6ff;
            border-left: 4px solid #3b82f6;
            padding: 1rem;
            border-radius: 12px;
            margin: 1.2rem 0;
        }

        footer {
            text-align: center;
            padding: 1.5rem 2rem;
            border-top: 1px solid #e2e8f0;
            color: #64748b;
            font-size: 0.85rem;
            background: #fafcff;
        }

        @media (max-width: 640px) {
            .hero h1 {
                font-size: 1.8rem;
            }

            .content {
                padding: 1.5rem;
            }
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="hero">
            <h1>📈 Stock Analysis Agent</h1>
            <p>AI-Powered Investment Assistant built with <strong>.NET Core 10 + Semantic Kernel + ML.NET</strong><br>Enterprise-grade Agent for stock analysis, risk detection, and intelligent memo generation</p>
            <div class="badge">🚀 Production Ready · Self-Hosted · Extensible</div>
        </div>
        <div class="content">
            <!-- Background & Goal -->
            <h2>🎯 Project Overview</h2>
            <p>The Stock Analysis Agent automates investment workflows: fetching stock data, computing technical indicators, detecting price anomalies, clustering risk levels, and generating natural-language investment memos. Built on the Microsoft AI stack, it demonstrates how .NET developers can build reliable, observable, and safe AI Agents for the financial domain.</p>

            <div class="highlight">
                ✔️ Stock data retrieval (price, volume, holdings)<br>
                ✔️ Technical indicators: MA5/10/20/30/60/120, RSI, annualized volatility<br>
                ✔️ Price anomaly detection (statistical change point)<br>
                ✔️ Risk clustering (Low/Medium/High) via K‑Means<br>
                ✔️ Next-day price prediction using ML.NET regression<br>
                ✔️ ONNX model inference for sentiment/risk scoring<br>
                ✔️ AI-generated investment memos (LLM via Semantic Kernel)<br>
                ✔️ Enterprise resilience: gateway, rate limiting, circuit breaker, structured logging, unit tests
            </div>

            <!-- Core Modules -->
            <h2>⚙️ Core Modules</h2>
            <div class="grid-3">
                <div class="card"><h4>📊 Stock Data Service</h4><p>Repository pattern + HttpClient + Polly policies. Fetches real-time/historical data from external APIs or mocks.</p></div>
                <div class="card"><h4>📐 Technical Indicators</h4><p>MAs (5–120), RSI, volatility. Pure math + ML.NET helpers.</p></div>
                <div class="card"><h4>⚠️ Anomaly Detection</h4><p>IID change point detection using ML.NET TimeSeries – identifies persistent shifts in price patterns.</p></div>
                <div class="card"><h4>🔖 Risk Clustering</h4><p>K‑Means on volatility, RSI, Sharpe ratio, max drawdown → Low/Medium/High risk groups.</p></div>
                <div class="card"><h4>📈 Price Prediction</h4><p>SDCA regression using last 5 days price + volume to predict next day closing price.</p></div>
                <div class="card"><h4>🧠 AI Memo Generation</h4><p>Semantic Kernel calls LLM (OpenAI/Zhipu) to produce professional investment advice based on indicators and risk score.</p></div>
                <div class="card"><h4>🛡️ Resilience & Gateway</h4><p>YARP reverse proxy + Polly (retry, circuit breaker, rate limiter, bulkhead).</p></div>
                <div class="card"><h4>📝 Structured Logging</h4><p>Serilog to console and rolling files, enriched with context (TraceId, machine, thread).</p></div>
                <div class="card"><h4>🧪 Testing & Evaluation</h4><p>xUnit + Moq unit tests (80%+ coverage); LangSmith offline evaluation for agent outputs.</p></div>
            </div>

            <!-- Technology Stack -->
            <h2>🛠️ Technology Stack</h2>
            <div class="grid-2">
                <div class="card"><h4>⭐ Core Framework</h4><span class="tech-badge">.NET Core 10</span> <span class="tech-badge">ASP.NET Core</span> <span class="tech-badge">C# 14</span><br><br><strong>AI Orchestration:</strong> Semantic Kernel 1.32<br><strong>Machine Learning:</strong> ML.NET 3.0.1 + ONNX Runtime 1.19</div>
                <div class="card"><h4>🔌 Infrastructure</h4><span class="tech-badge">YARP (Reverse Proxy)</span> <span class="tech-badge">Polly 8.4</span> <span class="tech-badge">Serilog</span> <span class="tech-badge">xUnit + Moq</span><br><span class="tech-badge">LangSmith (Python)</span> <span class="tech-badge">Docker</span></div>
            </div>
            <p>LLM support: OpenAI / Azure OpenAI / Zhipu AI (GLM-4-Flash) – any OpenAI‑compatible endpoint.</p>

            <!-- Architecture diagram (text) -->
            <h2>🏗️ System Architecture</h2>
            <div class="architecture">
                [Client / Browser]  →  YARP Gateway (:5000)  →  AgentService (:5001)
                │                         │
                │                         ├─ MemoController
                │                         ├─ StockService (plugin)
                │                         └─ MemoService (plugin)
                ↓                         ↓
                [Resilience Policies]     TechnicalIndicatorService
                (Rate limit/Circuit/Retry) ├─ ML.NET regression/clustering/time series
                └─ OnnxPredictionService
                └─ IStockRepository (HTTP + Polly)
            </div>

            <!-- Workflow -->
            <h2>🔄 Key Workflow: Generate Investment Memo</h2>
            <ol style="margin:1rem 0 1rem 1.8rem; background:#f8fafc; padding:1rem 1rem 1rem 2rem; border-radius:20px;">
                <li>Client <code>POST /api/agent/GenerateMemo</code> (stock code + date)</li>
                <li>YARP routes request to <code>MemoController</code></li>
                <li><code>StockService.GetStockAsync</code> calls Repository (automatic retry/circuit breaker)</li>
                <li><code>TechnicalIndicatorService.CalculateIndicators</code> computes MAs, RSI, volatility. If data sufficient → ML.NET prediction, anomaly detection, risk clustering</li>
                <li><code>MemoService.GenerateMemoAsync</code> calculates risk score (0–100) + invokes LLM via Semantic Kernel to generate natural language advice</li>
                <li>Returns structured <code>InvestmentMemo</code> JSON; Serilog logs full request context</li>
            </ol>

            <!-- Reliability -->
            <h2>🛡️ Enterprise Reliability Features</h2>
            <div class="grid-2">
                <div class="card"><h4>⏱️ Retry & Circuit Breaker</h4><p>Exponential backoff (max 3 retries) + circuit breaker (5 failures → open 30s, half‑open probe).</p></div>
                <div class="card"><h4>🚦 Rate Limiting & Bulkhead</h4><p>Token bucket: 100 req/min per IP; Bulkhead limits concurrent execution (max 10).</p></div>
                <div class="card"><h4>📋 Structured Logging</h4><p>Serilog to console and rolling files, enriched with machine name, thread ID, and TraceId for easy ELK integration.</p></div>
                <div class="card"><h4>🧪 Testing & Evaluation</h4><p>xUnit + Moq for unit tests; LangSmith evaluation scripts to monitor agent output quality.</p></div>
            </div>

            <!-- Project Structure -->
            <h2>📁 Project Structure</h2>
            <pre style="background:#1e293b; color:#e2e8f0; padding:1rem; border-radius:16px; overflow-x:auto; font-size:0.8rem;">
StockAnalysisAgent/
├── src/
│   ├── StockAgent.Gateway/               # YARP gateway
│   ├── StockAgent.AgentService/          # Core Agent (Controllers, Services)
│   ├── StockAgent.ML.Service/            # ML.NET + ONNX services
│   ├── StockAgent.Shared/                # DTOs, constants
│   └── StockAgent.Infrastructure/        # Repository, Polly policies
├── tests/
│   ├── StockAgent.UnitTests/             # xUnit + Moq tests
│   └── StockAgent.Evaluation/            # LangSmith evaluation scripts
├── docker-compose.yml
└── Dockerfile.agent / gateway
        </pre>

            <!-- Quick start -->
            <h2>🚀 Quick Start</h2>
            <div class="highlight">
                <strong>Prerequisites:</strong> .NET 10 SDK, Docker (optional), LLM API Key (OpenAI / Zhipu)<br>
                <strong>Run locally:</strong><br>
                <code>dotnet restore && dotnet build</code><br>
                <code>cd src/StockAgent.AgentService &amp;&amp; dotnet run --urls "http://localhost:5001"</code><br>
                <code>cd ../StockAgent.Gateway &amp;&amp; dotnet run --urls "http://localhost:5000"</code><br>
                <strong>Swagger UI:</strong> <a href="http://localhost:5001/swagger" target="_blank">http://localhost:5001/swagger</a>
            </div>

            <!-- Extensibility -->
            <h2>🔮 Extensibility & Future Directions</h2>
            <ul style="margin-left:1.5rem; margin-bottom:1rem;">
                <li>📡 Connect to real data sources (Wind, JoinQuant, Yahoo Finance) – implement <code>IStockRepository</code></li>
                <li>🤝 Multi-agent collaboration (Researcher, Trader, Risk) – Semantic Kernel Group Chat</li>
                <li>🖥️ Local LLM support (LLamaSharp / Ollama) to reduce API costs</li>
                <li>📱 Blazor / React frontend for conversational investment Q&A</li>
            </ul>

            <!-- Conclusion -->
            <h2>🎯 Summary</h2>
            <p><strong>Stock Analysis Agent</strong> is a production‑ready reference project demonstrating how .NET engineers can build robust AI Agents. It combines Semantic Kernel orchestration, local ML.NET models, enterprise resilience (Polly), structured logging (Serilog), and rigorous testing – all within the familiar C# ecosystem. The project proves that <strong>C#/.NET is a first‑class citizen for AI application development</strong>, offering performance, type safety, and maintainability for mission‑critical financial tasks.</p>
            <p style="margin-top:1rem; font-style:italic;">MIT Licensed – free for learning and commercial use.</p>
        </div>
        <footer>
            Stock Analysis Agent – Built with .NET 10, Semantic Kernel & ML.NET | AI for Investment
        </footer>
    </div>
</body>
</html>
