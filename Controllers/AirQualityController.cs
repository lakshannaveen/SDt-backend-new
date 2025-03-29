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
            string apiKey = _configuration["WAQI:ApiKey"];  // Get only the API key
            string city = "Colombo";  // You can make this dynamic if needed

            // ✅ Correct API URL format
            string apiUrl = $"https://api.waqi.info/feed/{city}/?token={apiKey}";

            _logger.LogInformation("Fetching air quality data from {url}", apiUrl);

            // ✅ Fetch Data
            var response = await _httpClient.GetStringAsync(apiUrl);
            var airQualityData = JsonSerializer.Deserialize<object>(response); // Adjust DTO if needed

            return Ok(airQualityData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching air quality data");
            return StatusCode(500, $"Error fetching data: {ex.Message}");
        }
    }

}
