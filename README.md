# WeatherForecastAPI

A robust REST API for weather forecasting, built with .NET and modern architecture principles.

## Tech Stack
-   **.NET 10** (Web API)
-   **Entity Framework Core** (SQL Server)
-   **Redis** (Caching)
-   **Docker & Docker Compose** (Containerization)

## Prerequisites
-   [Docker Desktop](https://www.docker.com/products/docker-desktop)
-   [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (optional, for local dev)

## Getting Started

### Option 1: Using Docker Compose

The easiest way to run the entire application stack:

```bash
git clone https://github.com/Salah1796/WeatherForecastAPI.git
cd WeatherForecastAPI
docker compose up -d
```

This will start:
- SQL Server (port 1433)
- Redis (port 6379)  
- WeatherForecast API (port 8080)

Access the API at: `http://localhost:8080`

The database migrations are applied automatically on startup.

### Option 2: Local Infrastructure (SQL Server + Redis)

If you have SQL Server and Redis installed locally:

#### 1. Update Connection Strings
Edit `WeatherForecast.Api/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=weatherforecastDb;Integrated Security=true;TrustServerCertificate=True",
  "Redis": "localhost:6379"
}
```

#### 2. Run the API
```bash
dotnet run --project WeatherForecast.Api
```

The database migrations are applied automatically on startup.

Access Swagger UI at: `http://localhost:5000/swagger` (check console output for exact port).