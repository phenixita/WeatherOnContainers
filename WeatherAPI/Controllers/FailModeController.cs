using Microsoft.AspNetCore.Mvc;

namespace WeatherAPI.Controllers
{
    [ApiController]
    [Route("[controller]")] 
    public class FailModeController : Controller
    {
        [HttpGet()]
        public string Get()
        {
            FailMode.Failing = !FailMode.Failing;

            return $"Fallimento impostato su: {FailMode.Failing}";
        }
    }



    public static class FailMode
    {
        public static bool Failing { get; set; }
    }

}
