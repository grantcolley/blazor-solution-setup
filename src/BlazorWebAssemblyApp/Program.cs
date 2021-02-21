using AppCore.Interface;
using AppServices;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BlazorWebAssemblyApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<BlazorShared.App>("#app");

            builder.Services.AddHttpClient<IWeatherForecastService, WeatherForecastService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:44303");
            });

            await builder.Build().RunAsync();
        }
    }
}
