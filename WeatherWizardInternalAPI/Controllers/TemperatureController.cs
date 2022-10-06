using Microsoft.AspNetCore.Mvc;
using System.Net;
using WeatherWizardInternalAPI.Controllers;
using WeatherWizardInternalAPI;

namespace WeatherWizardInternalAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TemperatureController : ControllerBase
    {
        
        private readonly ILogger<TemperatureController> _logger;

        public TemperatureController(ILogger<TemperatureController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetTemperature")]
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

            return Ok(Random.Shared.Next(-20, 50)); 
        }
    }
}