using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace FlightSearchApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightController : ControllerBase
    {
        [HttpPost("uploadProviderFiles")]
        public IActionResult UploadProviderFiles(IEnumerable<IFormFile> files)
        {
            return Ok();
        }
    }
}
