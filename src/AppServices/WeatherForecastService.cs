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
        private readonly HttpClient _httpClient;

        public WeatherForecastService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<WeatherForecast>> GetWeatherForecasts()
        {
            return await JsonSerializer.DeserializeAsync<IEnumerable<WeatherForecast>>
                (await _httpClient.GetStreamAsync($"WeatherForecast"), new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
    }
}
