using Core.Model;
using System.Collections.Generic;

namespace Core.Interface
{
    public interface IWeatherForecastRepository
    {
        IEnumerable<WeatherForecast> GetWeatherForecasts();
    }
}
