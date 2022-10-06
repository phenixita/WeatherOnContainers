using Polly;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WeatherMVC.Models;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;

namespace WeatherMVC.Controllers
{
    public class HomeController : Controller
    {
        const int MAX_RETRIES = 3;

        private readonly ILogger<HomeController> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        public HomeController(ILogger<HomeController> logger)
        {

            _logger = logger;
            _retryPolicy = Policy
                .Handle<HttpRequestException>(ex => ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    sleepDurations: new TimeSpan[] {
                        TimeSpan.FromMilliseconds(1000),
                        TimeSpan.FromMilliseconds(2000),
                        TimeSpan.FromMilliseconds(3000)
                    },
                    onRetry: (exception, timespan) =>
                    {
                        _logger.LogInformation($"Too many requests. Riprovo in {timespan}");
                    });
        }

        public async Task<IActionResult> Index()
        {
            ViewData["RunningOn"] = Environment.MachineName;

            ViewData["Meteo"] = await GetWeather();

            return View();
        }

        private async Task<string> GetWeather()
        {
            using var httpClient = new HttpClient();

            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await httpClient.GetAsync("http://weatherapi:5001/weatherforecast");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}