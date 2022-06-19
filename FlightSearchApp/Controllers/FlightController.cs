using FlightSearchApp.Dtos;
using FlightSearchApp.Models;
using FlightSearchApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;

namespace FlightSearchApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightController : ControllerBase
    {
        private readonly IFlightService _flightService;
        public FlightController(IFlightService fileService)
        {
            _flightService = fileService;
        }

        [HttpPost("uploadProviderFiles")]
        public IActionResult UploadProviderFiles(IEnumerable<IFormFile> files)
        {
            foreach(var file in files)
            {
                _flightService.AddFlights(Path.GetFileNameWithoutExtension(file.FileName), file.OpenReadStream());
            }
            return Ok();
        }

        [HttpPost("ClearData")]
        public IActionResult ClearData()
        {
            _flightService.ClearData();
            return Ok();
        }

        [HttpGet("Search")]
        public IActionResult SearchFlights([FromQuery] FlightSearchDto flightSearch)
        {
            return Ok(_flightService.GetFlightPlans(flightSearch.ToModel()));
        }
    }
}
