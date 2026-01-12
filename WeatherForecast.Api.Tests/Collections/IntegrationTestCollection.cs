using Xunit;
using WeatherForecast.Api.Tests.Fixtures;

namespace WeatherForecast.Api.Tests.Collections;

[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<WeatherForecastWebApplicationFactory>
{
}
