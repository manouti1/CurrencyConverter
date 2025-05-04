# CurrencyConverter API

An ASP.NET Core 8 Web API that provides currency conversion and exchange‐rate data via the [Frankfurter API](https://frankfurter.dev).  
Features:

- **Latest & historical rates** (with optional symbol filtering)  
- **Time‑series data** over arbitrary date ranges  
- **List of supported currencies**  
- **JWT authentication** & RBAC  
- **IP rate limiting** (via AspNetCoreRateLimit)  
- **Caching**, **retry** & **circuit breaker** (via Polly + IMemoryCache)  
- **Structured logging** (Serilog → Console & Seq)  
- **Distributed tracing** (OpenTelemetry → Zipkin)  
- **Global exception handling**  
- **API versioning** (v1)  
- **Unit & integration tests** (xUnit + Moq + Coverlet)  

---

## Table of Contents

1. [Prerequisites](#prerequisites)  
2. [Solution Structure](#solution-structure)  
3. [Configuration](#configuration)  
4. [Running the API](#running-the-api)  
5. [Running Zipkin](#running-zipkin)  
6. [Running Seq](#running-seq)  
7. [Testing](#testing)  
9. [License](#license)  

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)  
- **Either** Docker Desktop **or** Java 8+ (for Zipkin)  
- (Optional) [Seq](https://datalust.co/seq) instance for log aggregation  

---

## Solution Structure


**CurrencyConverter**/ 

**CurrencyConverter**/ 

CurrencyConverter.sln 

 - CurrencyConverter.Api/ **Web API, controllers & middleware** 
 - CurrencyConverter.Application/ **DTOs & ExchangeService** 
 - CurrencyConverter.Domain/ **Models & interfaces**
 - CurrencyConverter.Infrastructure/ **FrankfurterProvider, caching, retry policies**
 - CurrencyConverter.Tests/ **xUnit unit & integration tests**

## Installation & Build

1. **Clone the repo**  
   ```bash
   git clone https://github.com/your-org/CurrencyConverter.git
   cd CurrencyConverter
2. **Restore & Build**

```bash
cd src/CurrencyConverter.Api
dotnet restore
dotnet build --configuration Release


3. **Running the API**

## Set environment (optional)

```bash
# default is Development; to run production settings:
export ASPNETCORE_ENVIRONMENT=Production

## Run

```bash
dotnet run
