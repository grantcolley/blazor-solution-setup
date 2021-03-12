using AppCore.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppCore.Interface
{
    public interface IWeatherForecastService
    {
        Task<IEnumerable<WeatherForecast>> GetWeatherForecasts();
    }
}
