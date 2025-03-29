using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

[Route("api/dashboard")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DashboardController> _logger;
    private readonly IConfiguration _configuration;

    public DashboardController(HttpClient httpClient, ILogger<DashboardController> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet("aqi")]
    public async Task<IActionResult> GetAqiData()
    {
        try
        {
            string apiKey = _configuration["WAQI:ApiKey"];

            var colomboStations = new List<StationInfo>
        {
            new StationInfo { Name = "Colombo US Embassy", StationId = "@1456", Lat = 6.9271, Lng = 79.8612 },
            new StationInfo { Name = "Colombo Fort", StationId = "@1457", Lat = 6.9344, Lng = 79.8428 },
            new StationInfo { Name = "Kollupitiya", StationId = "@1458", Lat = 6.9106, Lng = 79.8553 },
            new StationInfo { Name = "Dehiwala", StationId = "@1459", Lat = 6.8409, Lng = 79.8756 }
        };

            var aqiData = new List<object>();

            foreach (var station in colomboStations)
            {
                try
                {
                    string waqiUrl = $"https://api.waqi.info/feed/{station.StationId}/?token={apiKey}";
                    var response = await _httpClient.GetAsync(waqiUrl);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var json = JsonDocument.Parse(content);

                    if (json.RootElement.TryGetProperty("data", out var data) &&
                        data.ValueKind != JsonValueKind.Null)
                    {
                        // Safely extract all values
                        var aqi = data.TryGetProperty("aqi", out var aqiElement) ? aqiElement.GetInt32() : -1;
                        var time = data.TryGetProperty("time", out var timeElement) ?
                                  timeElement.GetProperty("s").GetString() : DateTime.UtcNow.ToString("o");

                        var iaqi = data.TryGetProperty("iaqi", out var iaqiElement) ? iaqiElement : default;

                        aqiData.Add(new
                        {
                            location = station.Name,
                            coordinates = new[] { station.Lat, station.Lng },
                            aqi = aqi,
                            pm25 = TryGetIaqiValue(iaqi, "pm25"),
                            co = TryGetIaqiValue(iaqi, "co"),
                            temperature = TryGetIaqiValue(iaqi, "t"),
                            humidity = TryGetIaqiValue(iaqi, "h"),
                            lastUpdated = time
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing station {station.Name}");
                    // Add fallback data if station fails
                    aqiData.Add(new
                    {
                        location = station.Name,
                        coordinates = new[] { station.Lat, station.Lng },
                        aqi = -1,
                        pm25 = (double?)null,
                        co = (double?)null,
                        temperature = (double?)null,
                        humidity = (double?)null,
                        lastUpdated = DateTime.UtcNow.ToString("o")
                    });
                }
            }

            return Ok(new
            {
                status = "ok",
                data = aqiData,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching AQI data");
            return StatusCode(500, new
            {
                status = "error",
                message = ex.Message
            });
        }
    }

    private double? TryGetIaqiValue(JsonElement iaqi, string property)
    {
        if (iaqi.ValueKind == JsonValueKind.Undefined || !iaqi.TryGetProperty(property, out var prop))
            return null;

        return prop.TryGetProperty("v", out var value) ? value.GetDouble() : (double?)null;
    }
    private class StationInfo
    {
        public string Name { get; set; }
        public string StationId { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}