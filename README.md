# 📈 Stock Analysis Agent

AI-Powered Investment Assistant built with **.NET Core 10 + Semantic Kernel + ML.NET**  
Enterprise-grade Agent for stock analysis, risk detection, and intelligent memo generation

> 🚀 Production Ready · Self-Hosted · Extensible

---

## 🎯 Project Overview

The Stock Analysis Agent automates investment workflows: fetching stock data, computing technical indicators, detecting price anomalies, clustering risk levels, and generating natural-language investment memos. Built on the Microsoft AI stack, it demonstrates how .NET developers can build reliable, observable, and safe AI Agents for the financial domain.

- ✔️ Stock data retrieval (price, volume, holdings)
- ✔️ Technical indicators: MA5/10/20/30/60/120, RSI, annualized volatility
- ✔️ Price anomaly detection (statistical change point)
- ✔️ Risk clustering (Low/Medium/High) via K‑Means
- ✔️ Next-day price prediction using ML.NET regression
- ✔️ ONNX model inference for sentiment/risk scoring
- ✔️ AI-generated investment memos (LLM via Semantic Kernel)
- ✔️ Enterprise resilience: gateway, rate limiting, circuit breaker, structured logging, unit tests

---

## ⚙️ Core Modules

| Module | Description |
|--------|-------------|
| 📊 Stock Data Service | Repository pattern + HttpClient + Polly policies. Fetches real-time/historical data from external APIs or mocks. |
| 📐 Technical Indicators | MAs (5–120), RSI, volatility. Pure math + ML.NET helpers. |
| ⚠️ Anomaly Detection | IID change point detection using ML.NET TimeSeries – identifies persistent shifts in price patterns. |
| 🔖 Risk Clustering | K‑Means on volatility, RSI, Sharpe ratio, max drawdown → Low/Medium/High risk groups. |
| 📈 Price Prediction | SDCA regression using last 5 days price + volume to predict next day closing price. |
| 🧠 AI Memo Generation | Semantic Kernel calls LLM (OpenAI/Zhipu) to produce professional investment advice based on indicators and risk score. |
| 🛡️ Resilience & Gateway | YARP reverse proxy + Polly (retry, circuit breaker, rate limiter, bulkhead). |
| 📝 Structured Logging | Serilog to console and rolling files, enriched with context (TraceId, machine, thread). |
| 🧪 Testing & Evaluation | xUnit + Moq unit tests (80%+ coverage); LangSmith offline evaluation for agent outputs. |

---

## 🛠️ Technology Stack

- **Core Framework**: .NET Core 10, ASP.NET Core, C# 14
- **AI Orchestration**: Semantic Kernel 1.32
- **Machine Learning**: ML.NET 3.0.1 + ONNX Runtime 1.19
- **Infrastructure**: YARP (Reverse Proxy), Polly 8.4, Serilog, xUnit + Moq, LangSmith (Python), Docker

LLM support: OpenAI / Azure OpenAI / Zhipu AI (GLM-4-Flash) – any OpenAI‑compatible endpoint.

---

## 🏗️ System Architecture

```
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
```

---

## 🔄 Key Workflow: Generate Investment Memo

1. Client `POST /api/agent/GenerateMemo` (stock code + date)
2. YARP routes request to `MemoController`
3. `StockService.GetStockAsync` calls Repository (automatic retry/circuit breaker)
4. `TechnicalIndicatorService.CalculateIndicators` computes MAs, RSI, volatility. If data sufficient → ML.NET prediction, anomaly detection, risk clustering
5. `MemoService.GenerateMemoAsync` calculates risk score (0–100) + invokes LLM via Semantic Kernel to generate natural language advice
6. Returns structured `InvestmentMemo` JSON; Serilog logs full request context

---

## 🛡️ Enterprise Reliability Features

| Feature | Description |
|---------|-------------|
| ⏱️ Retry & Circuit Breaker | Exponential backoff (max 3 retries) + circuit breaker (5 failures → open 30s, half‑open probe). |
| 🚦 Rate Limiting & Bulkhead | Token bucket: 100 req/min per IP; Bulkhead limits concurrent execution (max 10). |
| 📋 Structured Logging | Serilog to console and rolling files, enriched with machine name, thread ID, and TraceId for easy ELK integration. |
| 🧪 Testing & Evaluation | xUnit + Moq for unit tests; LangSmith evaluation scripts to monitor agent output quality. |

---

## 📁 Project Structure

```
StockAnalysisAgent/
├── src/
│   ├── Gateway/               # YARP gateway
│   ├── AgentService/          # Core Agent (Controllers, Services)
│   ├── ML.Service/            # ML.NET + ONNX services
│   ├── Shared/                # DTOs, constants
│   └── Infrastructure/        # Repository, Polly policies
├── tests/
│   ├── UnitTests/             # xUnit + Moq tests
│   └── Evaluation/            # LangSmith evaluation scripts
├── docker-compose.yml
└── Dockerfile.agent / gateway
```

---

## 🚀 Quick Start

**Prerequisites:** .NET 10 SDK, Docker (optional), LLM API Key (OpenAI / Zhipu)

**Run locally:**

```bash
dotnet restore && dotnet build
cd src/AgentService && dotnet run --urls "http://localhost:5001"
cd ../Gateway && dotnet run --urls "http://localhost:5000"
```

**Swagger UI:** [http://localhost:5001/swagger](http://localhost:5001/swagger)

---

## 🔮 Extensibility & Future Directions

- 📡 Connect to real data sources (Wind, JoinQuant, Yahoo Finance) – implement `IStockRepository`
- 🤝 Multi-agent collaboration (Researcher, Trader, Risk) – Semantic Kernel Group Chat
- 🖥️ Local LLM support (LLamaSharp / Ollama) to reduce API costs
- 📱 Blazor / React frontend for conversational investment Q&A

---

## 🎯 Summary

**Stock Analysis Agent** is a production‑ready reference project demonstrating how .NET engineers can build robust AI Agents. It combines Semantic Kernel orchestration, local ML.NET models, enterprise resilience (Polly), structured logging (Serilog), and rigorous testing – all within the familiar C# ecosystem. The project proves that **C#/.NET is a first‑class citizen for AI application development**, offering performance, type safety, and maintainability for mission‑critical financial tasks.

> MIT Licensed – free for learning and commercial use.

---

*Stock Analysis Agent – Built with .NET 10, Semantic Kernel & ML.NET | AI for Investment*

---
