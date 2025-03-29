using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

[Route("api/airquality")]
[ApiController]
public class AirQualityController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AirQualityController> _logger;
    private readonly IConfiguration _configuration;

    public AirQualityController(HttpClient httpClient, ILogger<AirQualityController> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetAirQualityData()
    {
        try
        {
            string apiKey = _configuration["WAQI:ApiKey"];

            // List of station IDs in Colombo district
            var colomboStations = new Dictionary<string, string>
        {
            {"Colombo US Embassy", "@1456"},
            {"Colombo Fort", "@1457"},
            // Add more stations as needed
        };

            var stationData = new List<object>();

            foreach (var station in colomboStations)
            {
                string apiUrl = $"https://api.waqi.info/feed/{station.Value}/?token={apiKey}";

                var response = await _httpClient.GetAsync(apiUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Deserialize and add to our list
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

                if (result.TryGetProperty("data", out var data))
                {
                    stationData.Add(new
                    {
                        location = station.Key,
                        data = data
                    });
                }
            }

            return Ok(new
            {
                status = "ok",
                stations = stationData
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching air quality data");
            return StatusCode(500, new { status = "error", message = ex.Message });
        }
    }

}
