using System;
using System.Collections.Generic;
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

            // List of major monitoring stations in Colombo district
            var colomboStations = new Dictionary<string, string>
            {
                {"Colombo US Embassy", "@1456"},
                {"Colombo Fort", "@1457"},
                {"Kollupitiya", "@1458"},
                {"Dehiwala", "@1459"},
                {"Mount Lavinia", "@1460"},
                {"Pettah", "@1461"},
                {"Nugegoda", "@1462"},
                {"Battaramulla", "@1463"}
            };

            var stationDataList = new List<object>();

            foreach (var station in colomboStations)
            {
                try
                {
                    string apiUrl = $"https://api.waqi.info/feed/{station.Value}/?token={apiKey}";

                    _logger.LogInformation($"Fetching data for {station.Key} from {apiUrl}");

                    var response = await _httpClient.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonDocument = JsonDocument.Parse(responseContent);

                    // Check if the response contains valid data
                    if (jsonDocument.RootElement.TryGetProperty("data", out var dataElement))
                    {
                        stationDataList.Add(new
                        {
                            location = station.Key,
                            data = JsonSerializer.Deserialize<object>(responseContent)
                        });
                    }
                    else
                    {
                        _logger.LogWarning($"No data property found for station {station.Key}");
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    _logger.LogError(httpEx, $"Error fetching data for station {station.Key}");
                    // Continue with next station even if one fails
                    continue;
                }
            }

            return Ok(new
            {
                status = "ok",
                stations = stationDataList,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in air quality data controller");
            return StatusCode(500, new
            {
                status = "error",
                message = ex.Message,
                details = ex.InnerException?.Message
            });
        }
    }

    // Optional: Add endpoint to get specific station data
    [HttpGet("{stationId}")]
    public async Task<IActionResult> GetStationData(string stationId)
    {
        try
        {
            string apiKey = _configuration["WAQI:ApiKey"];
            string apiUrl = $"https://api.waqi.info/feed/{stationId}/?token={apiKey}";

            var response = await _httpClient.GetStringAsync(apiUrl);
            return Ok(JsonSerializer.Deserialize<object>(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching data for station {stationId}");
            return StatusCode(500, $"Error fetching station data: {ex.Message}");
        }
    }
}