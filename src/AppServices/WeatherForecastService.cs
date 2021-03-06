using AppCore.Interface;
using AppCore.Model;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppServices
{
    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly HttpClient httpClient;
        private readonly TokenProvider tokenProvider;
        private readonly bool useAccessToken;

        public WeatherForecastService(HttpClient httpClient) : this(httpClient, null, false)
        {
        }

        public WeatherForecastService(HttpClient httpClient, TokenProvider tokenProvider, bool useAccessToken)
        {
            this.httpClient = httpClient;
            this.tokenProvider = tokenProvider;
            this.useAccessToken = useAccessToken;
        }

        public async Task<IEnumerable<WeatherForecast>> GetWeatherForecasts()
        {
            if (useAccessToken)
            {
                var token = tokenProvider.AccessToken;
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            }

            return await JsonSerializer.DeserializeAsync<IEnumerable<WeatherForecast>>
                (await httpClient.GetStreamAsync($"WeatherForecast"), new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
    }
}
