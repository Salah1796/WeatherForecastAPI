
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WeatherForecast.Api/WeatherForecast.Api.csproj", "WeatherForecast.Api/"]
COPY ["WeatherForecast.Application/WeatherForecast.Application.csproj", "WeatherForecast.Application/"]
COPY ["WeatherForecast.Domain/WeatherForecast.Domain.csproj", "WeatherForecast.Domain/"]
COPY ["WeatherForecast.Infrastructure/WeatherForecast.Infrastructure.csproj", "WeatherForecast.Infrastructure/"]
RUN dotnet restore "./WeatherForecast.Api/WeatherForecast.Api.csproj"
COPY . .
WORKDIR "/src/WeatherForecast.Api"
RUN dotnet build "./WeatherForecast.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WeatherForecast.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WeatherForecast.Api.dll"]
