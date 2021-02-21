﻿using AppCore.Interface;
using AppCore.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IWeatherForecastRepository _weatherForecastRepository;

        public WeatherForecastController(IWeatherForecastRepository weatherForecastRepository, ILogger<WeatherForecastController> logger)
        {
            _weatherForecastRepository = weatherForecastRepository;
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return _weatherForecastRepository.GetWeatherForecasts();
        }
    }
}
