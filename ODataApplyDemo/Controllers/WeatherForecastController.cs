using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataApplyDemo.Models;
using System.Text.Json.Serialization;

namespace ODataApplyDemo.Controllers;

public class WeatherForecastController : ODataController
{
    private static readonly string[] Summaries = {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    // https://localhost:7109/odata/WeatherForecast?$apply=groupby((TemperatureC))
    public IActionResult Get(ODataQueryOptions<WeatherForecast> options)
    {
        var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        });

        var result = options.ApplyTo(forecast.AsQueryable());

        return Ok(result);
    }
}

//public class WeatherForecastController : ODataController
//{
//    private static readonly string[] Summaries = {
//        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
//    };

//    public IActionResult Get(ODataQueryOptions<WeatherForecast> options)
//    {
//        var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
//        {
//            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//            TemperatureC = Random.Shared.Next(-20, 55),
//            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
//        });

//        return Ok(options.ApplyTo(forecast.AsQueryable()).ToODataResponse(Request.FullUrl()));
//    }
//}

//public static class HttpRequestExtensions
//{
//    public static string FullUrl(this HttpRequest request)
//        => $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
//}

//public static class ODataExtensions
//{
//    public static ODataResponseWrapper ToODataResponse(this IQueryable source, string url)
//    {
//        ArgumentNullException.ThrowIfNull(source, nameof(source));

//        return new()
//        {
//            Context = url,
//            Value = source
//        };
//    }
//}

//public class ODataResponseWrapper
//{
//    [JsonPropertyName("@odata.context")]
//    public required string Context { get; set; }

//    [JsonPropertyName("value")]
//    public required IQueryable Value { get; set; }
//}