using System.Text.Json.Serialization;

namespace ODataApplyDemo.Models;

public class OdataResponseWrapper
{
    [JsonPropertyName("@odata.context")]
    public required string Context { get; set; }

    [JsonPropertyName("value")]
    public required IQueryable Value { get; set; }
}