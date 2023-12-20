namespace ODataApplyDemo.Models;

public class WeatherForecast
{
    public string Id { get; set; } = string.Empty;
    public DateOnly? Date { get; set; }

    public int? TemperatureC { get; set; }

    public int? TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary { get; set; }
}
