using AppCore.Model;
using System.Collections.Generic;

namespace AppCore.Interface
{
    public interface IWeatherForecastRepository
    {
        IEnumerable<WeatherForecast> GetWeatherForecasts();
    }
}
