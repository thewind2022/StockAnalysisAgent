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
