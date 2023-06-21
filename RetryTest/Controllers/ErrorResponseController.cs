using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace RetryTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ErrorResponseController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<ErrorResponseController> _logger;

        public ErrorResponseController(ILogger<ErrorResponseController> logger)
        {
            _logger = logger;
        }

        [Route("GetErrorResponse")]
        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            this._logger.LogInformation("GetErrorResponse called");
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [Route("GetErrorResponse429")]
        [HttpGet]
        public ActionResult<IEnumerable<WeatherForecast>> Get429()
        {
            //return new StatusCodeResult(429);
            this._logger.LogInformation("GetErrorResponse429 called");
            return this.Problem("ToOmanyRequests", "Too many requests", (int)HttpStatusCode.TooManyRequests, "Fail on purpose");

            //return this.StatusCode((int)HttpStatusCode.ServiceUnavailable, "Fail on purpose");
        }

        [Route("GetErrorResponse503")]
        [HttpGet]
        public ActionResult<IEnumerable<WeatherForecast>> Get503()
        {
            //return new StatusCodeResult(429);
            this._logger.LogInformation("GetErrorResponse503 called");
            return this.Problem("ServiceUnavail", "Too many requests", (int)HttpStatusCode.ServiceUnavailable, "Fail on purpose");

            //return this.StatusCode((int)HttpStatusCode.ServiceUnavailable, "Fail on purpose");
        }

        [Route("GetErrorResponseSlow")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WeatherForecast>>> GetSlow()
        {
            this._logger.LogInformation("GetErrorResponseSlow called");

            //return new StatusCodeResult(429);
            await Task.Delay(TimeSpan.FromSeconds(6));
            return this.Problem("ServiceUnavail", "Too many requests", (int)HttpStatusCode.ServiceUnavailable, "Fail on purpose");

            //return this.StatusCode((int)HttpStatusCode.ServiceUnavailable, "Fail on purpose");
        }
    }
}