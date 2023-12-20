using Microsoft.AspNetCore.OData.Query;
using ODataApplyDemo.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ODataApplyDemo.Attributes;

public class CustomEnableQuery : EnableQueryAttribute
{
    public override IQueryable ApplyQuery(IQueryable queryable, ODataQueryOptions queryOptions)
    {
        var query = queryOptions.ApplyTo(queryable);
        if (queryOptions.Apply is null)
        {
            return query;
        }
        var jsonOptions = new JsonSerializerOptions
        {
            Converters = { new AggregationWrapperConverter() }
        };
        var jsonString = JsonSerializer.Serialize(query, jsonOptions);
        var result = JsonSerializer.Deserialize<List<WeatherForecast>>(jsonString).AsQueryable();

        return result;
    }
}

public class AggregationWrapperConverter : JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, object obj, JsonSerializerOptions options)
    {
        var container = obj.GetType().GetProperty("GroupByContainer")?.GetValue(obj, null);
        if (container != null)
        {
            string name = container.GetType().GetProperty("Name")?.GetValue(container, null)?.ToString();
            var value = container.GetType().GetProperty("Value")?.GetValue(container, null);

            if (name is not null && value is not null)
            {
                var x = new Dictionary<string, object>
                {
                    { name, value }
                };
                var jsonDict = Newtonsoft.Json.JsonConvert.SerializeObject(x);

                using (var jsonDoc = JsonDocument.Parse(jsonDict))
                {
                    writer.WriteStartObject();

                    foreach (var element in jsonDoc.RootElement.EnumerateObject())
                    {
                        writer.WritePropertyName(element.Name);
                        switch (element.Value.ValueKind)
                        {
                            case JsonValueKind.Number:
                                writer.WriteNumberValue(element.Value.GetDecimal());
                                break;
                            case JsonValueKind.String:
                                writer.WriteStringValue(element.Value.GetString());
                                break;
                            // case további típusok
                            default:
                                // hibakezelést
                                break;
                        }
                    }

                    writer.WriteEndObject();
                }
            }
        }
    }
}
