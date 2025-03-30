using Microsoft.AspNetCore.Mvc;
using sdt_backend.net.Models;
using System;
using System.Threading.Tasks;

namespace sdt_backend.net.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StationDataController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StationDataController> _logger;

        public StationDataController(AppDbContext context, ILogger<StationDataController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddStationData([FromBody] StationDataDto stationDataDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Parse and convert to UTC
                if (!DateTime.TryParse(stationDataDto.Timestamp, out var timestamp))
                {
                    return BadRequest("Invalid timestamp format");
                }

                // Ensure timestamp is in UTC
                if (timestamp.Kind != DateTimeKind.Utc)
                {
                    timestamp = timestamp.ToUniversalTime();
                }

                var station = new Station
                {
                    StationName = stationDataDto.StationName,
                    Latitude = stationDataDto.Latitude,
                    Longitude = stationDataDto.Longitude,
                    Timestamp = timestamp,
                    AQI = stationDataDto.AQI,
                    PM25 = stationDataDto.PM25,
                    CO = stationDataDto.CO,
                    Temperature = stationDataDto.Temperature,
                    Humidity = stationDataDto.Humidity
                };

                _context.AirQualityStations.Add(station);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"New station data added: {station.StationName} at {station.Timestamp}");

                return Ok(new
                {
                    message = "Station data added successfully",
                    stationId = station.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding station data");
                return StatusCode(500, new
                {
                    message = "An error occurred while adding station data",
                    error = ex.Message
                });
            }
        }

        public class StationDataDto
        {
            public string StationName { get; set; }
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
            public string Timestamp { get; set; }
            public int AQI { get; set; }
            public double PM25 { get; set; }
            public double CO { get; set; }
            public double Temperature { get; set; }
            public double Humidity { get; set; }
        }
    }
}