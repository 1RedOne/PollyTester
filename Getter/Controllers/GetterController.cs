using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Reflection.PortableExecutable;

namespace Getter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GetterController : ControllerBase
    {
        private readonly HttpGetter _getter;
        private readonly ILogger<GetterController> _logger;

        public GetterController(HttpGetter getter, ILogger<GetterController> logger)
        {
            _getter = getter;
            _logger = logger;
        }

        [Route("SendGet429")]
        [HttpGet]
        public ActionResult<string> Get()
        {
            //http://localhost:5114/ErrorResponse/GetErrorResponse503

            _ = _logger.BeginScope($"Controller - Requesting getter to go retrieve");
            //http://localhost:5114/ErrorResponse/GetErrorResponse429

            var url = "http://localhost:5114/ErrorResponse/GetErrorResponse429";
            var result = this._getter.GetAsync(url);

            return result.Result;
        }

        //http://localhost:5114/ErrorResponse/GetErrorResponse503
        [Route("SendGet503")]
        [HttpGet]
        public ActionResult<string> Get503()
        {
            _ = _logger.BeginScope($"Controller - Requesting getter to go retrieve");

            var url = "http://localhost:5114/ErrorResponse/GetErrorResponse503";
            var result = this._getter.GetAsync(url);

            return result.Result;
        }

        [Route("SendToSlowEndpoint")]
        [HttpGet]
        public ActionResult<string> GetSlow()
        {
            _ = _logger.BeginScope($"Controller - Requesting getter to go retrieve");

            var url = "http://localhost:5114/ErrorResponse/GetErrorResponseSlow";
            var result = this._getter.GetAsync(url);

            return result.Result;
        }
    }
}