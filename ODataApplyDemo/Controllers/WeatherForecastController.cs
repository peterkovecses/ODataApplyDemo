using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataApplyDemo.Attributes;
using ODataApplyDemo.Models;

namespace ODataApplyDemo.Controllers;

[CustomEnableQuery]
public class WeatherForecastController : ODataController
{
    private static readonly string[] Summaries = {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    // https://localhost:7109/odata/WeatherForecast?$apply=groupby((TemperatureC))
    // https://localhost:7109/odata/WeatherForecast?$apply=groupby((TemperatureC),aggregate($count%20as%20Count))
    public IActionResult Get(ODataQueryOptions<WeatherForecast> queryOptions)
    {
        var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Id = Guid.NewGuid().ToString(),
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        });

        return Ok(forecast);
    }
}