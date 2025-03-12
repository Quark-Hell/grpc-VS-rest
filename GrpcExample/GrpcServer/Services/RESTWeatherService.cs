using Microsoft.AspNetCore.Mvc;

namespace WeatherServer.Controllers
{
    [ApiController]
    [Route("api/weather")]
    public class WeatherController : ControllerBase
    {
        private readonly Random _random = new Random();

        // GET /api/weather/current?city=Москва
        [HttpGet("current")]
        public IActionResult GetCurrentWeather([FromQuery] string city)
        {
            var weatherList = new List<WeatherResponse>();

            for (int i = 0; i < 5; i++)
            {
                var weather = new WeatherResponse
                {
                    City = city,
                    Temperature = _random.Next(10, 30),
                    Description = "Солнечно"
                };
                weatherList.Add(weather);
            }

            return Ok(weatherList);
        }

        // GET /api/weather/monitor?city=Москва
        [HttpGet("monitor")]
        public async Task MonitorWeather([FromQuery] string city)
        {
            Response.ContentType = "text/event-stream";

            while (!HttpContext.RequestAborted.IsCancellationRequested)
            {
                var weatherList = new List<WeatherResponse>();

                for (int i = 0; i < 5; i++)
                {
                    var weather = new WeatherResponse
                    {
                        City = city,
                        Temperature = _random.Next(10, 30),
                        Description = "Солнечно"
                    };
                    weatherList.Add(weather);
                }

                var jsonResponse = System.Text.Json.JsonSerializer.Serialize(weatherList);

                await Response.WriteAsync($"data: {jsonResponse}\n\n");
                await Response.Body.FlushAsync();

                await Task.Delay(1000);
            }
        }

        // GET /api/weather/test?city=Москва
        [HttpGet("test")]
        public IActionResult TestRest([FromQuery] string city)
        {
            var weatherList = new List<WeatherResponse>();

            for (int i = 0; i < 10000; i++)
            {
                var weather = new WeatherResponse
                {
                    City = city,
                    Temperature = _random.Next(10, 30),
                    Description = "Солнечно"
                };
                weatherList.Add(weather);
            }

            return Ok(weatherList);
        }

    }


    public class WeatherResponse
    {
        public string City { get; set; }
        public double Temperature { get; set; }
        public string Description { get; set; }
    }
}
