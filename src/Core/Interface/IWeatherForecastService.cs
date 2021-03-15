using Core.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interface
{
    public interface IWeatherForecastService
    {
        Task<IEnumerable<WeatherForecast>> GetWeatherForecasts();
    }
}
