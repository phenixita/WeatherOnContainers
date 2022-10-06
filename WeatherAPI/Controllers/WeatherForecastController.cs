using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Retry;
using System.Net;

namespace WeatherAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly AsyncRetryPolicy _retryPolicy;


        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
            _retryPolicy = Policy
                   .Handle<HttpRequestException>(ex => ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                   .WaitAndRetryAsync(
                       sleepDurations: new TimeSpan[] {
                                TimeSpan.FromMilliseconds(200),
                                TimeSpan.FromMilliseconds(300),
                                TimeSpan.FromMilliseconds(600)
                       },
                       onRetry: (exception, timespan) =>
                       {
                           _logger.LogInformation($"Too many requests. Riprovo in {timespan}");
                       });

        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IActionResult Get()
        {
            if (FailMode.Failing)
            {
                throw new InvalidOperationException("Fail mode enabled");
            }

            if (Random.Shared.Next() % 3 == 0)
            {
                return StatusCode((int)HttpStatusCode.TooManyRequests);
            }

            return Ok(Enumerable.Range(1, 5).Select(async (index) => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = int.Parse(await GetTemperature()),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray());
        }

        private async Task<string> GetTemperature()
        {
            using var httpClient = new HttpClient();

            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await httpClient.GetAsync("http://weatherwizardinternalapi:6001/Temperature");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            });
        }
    }
}